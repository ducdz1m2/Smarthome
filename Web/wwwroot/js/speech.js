// Speech Service JavaScript Interop
// Provides TTS and STT functionality for Blazor components

let mediaRecorder = null;
let audioChunks = [];
let stream = null;
let audioContext = null;
let analyser = null;
let source = null;
let silenceTimer = null;
let vadCheckInterval = null;
let dotNetCallback = null;

const silenceThreshold = -40; // dB
const silenceDuration = 2000; // ms - 2 seconds of silence to stop

// TTS - Text to Speech
window.speakText = async (speechServiceUrl, text, speed = 1.0) => {
    try {
        const response = await fetch(`${speechServiceUrl}/tts`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                text: text,
                speed: speed,
                speaker_id: 0
            })
        });

        if (!response.ok) {
            throw new Error('TTS request failed');
        }

        const blob = await response.blob();
        const audioUrl = URL.createObjectURL(blob);
        const audio = new Audio(audioUrl);
        
        await audio.play();

        // Cleanup URL after playback
        audio.onended = () => {
            URL.revokeObjectURL(audioUrl);
        };

        audio.onerror = () => {
            URL.revokeObjectURL(audioUrl);
            throw new Error('Audio playback failed');
        };
    } catch (error) {
        console.error('TTS error:', error);
        throw error;
    }
};

// Check Speech Service Health
window.checkSpeechServiceHealth = async (speechServiceUrl) => {
    try {
        const response = await fetch(`${speechServiceUrl}/health`);
        return response.ok;
    } catch (error) {
        console.error('Health check error:', error);
        return false;
    }
};

// STT - Speech to Text
window.startRecording = async (speechServiceUrl, callback) => {
    try {
        // Store speech service URL globally
        window.speechServiceUrl = speechServiceUrl;
        dotNetCallback = callback;
        console.log('Recording started, callback set:', callback !== null);

        // Get microphone access
        stream = await navigator.mediaDevices.getUserMedia({
            audio: {
                echoCancellation: true,
                noiseSuppression: true,
                autoGainControl: true
            }
        });

        // Setup AudioContext for VAD
        audioContext = new (window.AudioContext || window.webkitAudioContext)();
        analyser = audioContext.createAnalyser();
        analyser.fftSize = 2048;
        analyser.smoothingTimeConstant = 0.8;

        source = audioContext.createMediaStreamSource(stream);
        source.connect(analyser);

        // Setup MediaRecorder
        mediaRecorder = new MediaRecorder(stream, {
            mimeType: 'audio/webm;codecs=opus'
        });

        audioChunks = [];

        mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) {
                audioChunks.push(event.data);
            }
        };

        // Resume AudioContext if needed
        if (audioContext.state === 'suspended') {
            await audioContext.resume();
        }

        mediaRecorder.start(100);

        // Start VAD after a short delay
        setTimeout(() => startVAD(), 300);
    } catch (error) {
        console.error('Microphone access error:', error);
        cleanup();
        throw error;
    }
};

// Voice Activity Detection
function startVAD() {
    if (!analyser) return;

    const bufferLength = analyser.frequencyBinCount;
    const dataArray = new Uint8Array(bufferLength);
    let silenceStart = null;
    let speechDetected = false;

    vadCheckInterval = setInterval(() => {
        if (!analyser || !mediaRecorder || mediaRecorder.state === 'inactive') {
            stopVAD();
            return;
        }

        analyser.getByteTimeDomainData(dataArray);

        // Calculate RMS (Root Mean Square)
        let sum = 0;
        for (let i = 0; i < dataArray.length; i++) {
            const sample = (dataArray[i] - 128) / 128;
            sum += sample * sample;
        }
        const rms = Math.sqrt(sum / dataArray.length);

        // Convert to dB
        const db = 20 * Math.log10(rms || 0.0001);

        if (db > silenceThreshold) {
            // Sound detected
            speechDetected = true;
            silenceStart = null;
        } else if (speechDetected) {
            // Silence after speech
            if (silenceStart === null) {
                silenceStart = Date.now();
            } else {
                const silenceTime = Date.now() - silenceStart;
                if (silenceTime >= silenceDuration) {
                    // Enough silence detected -> stop recording
                    console.log('VAD: Silence detected for', silenceTime, 'ms');
                    stopVAD();
                    stopAndTranscribe().catch(err => console.error('VAD transcription error:', err));
                }
            }
        }
    }, 100);
}

function stopVAD() {
    if (vadCheckInterval) {
        clearInterval(vadCheckInterval);
        vadCheckInterval = null;
    }
}

window.stopAndTranscribe = async () => {
    stopVAD();

    return new Promise((resolve, reject) => {
        if (!mediaRecorder || mediaRecorder.state === 'inactive') {
            cleanup();
            resolve('');
            return;
        }

        mediaRecorder.onstop = async () => {
            try {
                const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });

                if (audioBlob.size < 1000) {
                    cleanup();
                    const emptyText = '';
                    if (dotNetCallback) {
                        try {
                            await dotNetCallback.invokeMethodAsync('OnTranscriptionComplete', emptyText);
                        } catch (err) {
                            console.error('DotNet callback error:', err);
                        }
                    }
                    resolve(emptyText);
                    return;
                }

                const text = await sendToSTT(audioBlob);
                cleanup();
                
                // Call DotNet callback if it exists
                if (dotNetCallback) {
                    try {
                        console.log('Calling DotNet callback with text:', text);
                        await dotNetCallback.invokeMethodAsync('OnTranscriptionComplete', text);
                        console.log('DotNet callback completed');
                    } catch (err) {
                        console.error('DotNet callback error:', err);
                    }
                }
                
                resolve(text);
            } catch (error) {
                console.error('Transcription error:', error);
                cleanup();
                if (dotNetCallback) {
                    try {
                        await dotNetCallback.invokeMethodAsync('OnTranscriptionComplete', '');
                    } catch (err) {
                        console.error('DotNet callback error during cleanup:', err);
                    }
                }
                reject(error);
            }
        };

        mediaRecorder.stop();
    });
};

window.cancelRecording = () => {
    stopVAD();
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
    }
    cleanup();
};

async function sendToSTT(audioBlob) {
    const formData = new FormData();
    formData.append('audio', audioBlob, 'recording.webm');

    try {
        const speechServiceUrl = window.speechServiceUrl || 'http://localhost:8003';
        console.log('Sending audio to STT service:', speechServiceUrl, 'Blob size:', audioBlob.size);
        
        const response = await fetch(`${speechServiceUrl}/stt`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            console.error('STT request failed with status:', response.status);
            throw new Error('STT request failed');
        }

        const result = await response.json();
        console.log('STT response:', result);
        return result.text || '';
    } catch (error) {
        console.error('STT API error:', error);
        throw error;
    }
}

function cleanup() {
    stopVAD();

    if (source) {
        source.disconnect();
        source = null;
    }

    if (audioContext && audioContext.state !== 'closed') {
        audioContext.close();
        audioContext = null;
    }

    analyser = null;

    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        stream = null;
    }

    mediaRecorder = null;
    audioChunks = [];
    dotNetCallback = null;
}
