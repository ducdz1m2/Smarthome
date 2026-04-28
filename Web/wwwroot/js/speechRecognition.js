// Speech Recognition & TTS — Audio capture + playback
window.speechRecognition = (function () {
    console.log('[speechRecognition] Module loaded');
    let mediaRecorder = null;
    let audioChunks = [];
    let dotNetRef = null;
    let currentAudio = null; // for TTS playback

    return {
        // Test function
        test: function () {
            console.log('[speechRecognition] test() called - JS interop is working!');
            return 'JS interop OK';
        },

        // ── STT: ghi âm từ microphone ─────────────────────────────────────────

        startRecording: async function (dotNetReference) {
            try {
                console.log('[speechRecognition] startRecording called');
                dotNetRef = dotNetReference;
                audioChunks = [];

                const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                console.log('[speechRecognition] Microphone access granted');

                // Ưu tiên webm/opus, fallback sang bất kỳ format nào được hỗ trợ
                const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
                    ? 'audio/webm;codecs=opus'
                    : MediaRecorder.isTypeSupported('audio/webm')
                        ? 'audio/webm'
                        : '';

                mediaRecorder = mimeType
                    ? new MediaRecorder(stream, { mimeType })
                    : new MediaRecorder(stream);
                console.log('[speechRecognition] MediaRecorder created with mimeType:', mediaRecorder.mimeType);

                mediaRecorder.ondataavailable = (event) => {
                    console.log('[speechRecognition] ondataavailable: size=', event.data.size);
                    if (event.data.size > 0) {
                        audioChunks.push(event.data);
                    }
                };

                mediaRecorder.onstop = async () => {
                    console.log('[speechRecognition] onstop triggered, audioChunks.length=', audioChunks.length);
                    // Giải phóng microphone
                    stream.getTracks().forEach(track => track.stop());

                    if (!dotNetRef) {
                        console.log('[speechRecognition] onstop: dotNetRef is null, skipping callback');
                        return; // đã bị cancel
                    }

                    const audioBlob = new Blob(audioChunks, {
                        type: mediaRecorder.mimeType || 'audio/webm'
                    });
                    console.log('[speechRecognition] audioBlob size:', audioBlob.size);

                    // Gửi audio lên speech service qua HTTP POST thay vì qua Blazor JS interop
                    try {
                        console.log('[speechRecognition] Sending audio to /api/speech/stt via HTTP...');
                        const formData = new FormData();
                        const extension = mediaRecorder.mimeType.includes('wav') ? 'recording.wav'
                                          : mediaRecorder.mimeType.includes('ogg') ? 'recording.ogg'
                                          : 'recording.webm';
                        formData.append('audio', audioBlob, extension);

                        const response = await fetch('/api/speech/stt', {
                            method: 'POST',
                            body: formData
                        });

                        if (response.ok) {
                            const result = await response.json();
                            console.log('[speechRecognition] STT result:', result);
                            // Gửi text về .NET qua callback nhẹ
                            await dotNetRef.invokeMethodAsync('OnTranscriptionResult', result.text || '');
                        } else {
                            console.error('[speechRecognition] STT failed:', response.status, response.statusText);
                            await dotNetRef.invokeMethodAsync('OnTranscriptionResult', '');
                        }
                    } catch (err) {
                        console.error('[speechRecognition] STT error:', err);
                        await dotNetRef.invokeMethodAsync('OnTranscriptionResult', '');
                    }

                    console.log('[speechRecognition] Callback completed');
                    dotNetRef = null;
                };

                mediaRecorder.start();
                console.log('[speechRecognition] MediaRecorder started');
                return true;
            } catch (err) {
                console.error('[speechRecognition] startRecording failed:', err);
                dotNetRef = null;
                return false;
            }
        },

        stopRecording: function () {
            console.log('[speechRecognition] stopRecording called, mediaRecorder state:', mediaRecorder ? mediaRecorder.state : 'null');
            if (mediaRecorder && mediaRecorder.state === 'recording') {
                mediaRecorder.stop();
                console.log('[speechRecognition] mediaRecorder.stop() called');
                return true;
            }
            console.log('[speechRecognition] mediaRecorder not in recording state');
            return false;
        },

        cancelRecording: function () {
            if (mediaRecorder && mediaRecorder.state !== 'inactive') {
                // Xóa callback trước khi stop để không invoke OnAudioCaptured
                dotNetRef = null;
                mediaRecorder.onstop = () => {
                    try {
                        mediaRecorder.stream?.getTracks().forEach(t => t.stop());
                    } catch (_) { }
                };
                mediaRecorder.stop();
            }
            dotNetRef = null;
            audioChunks = [];
        },

        isRecording: function () {
            return mediaRecorder !== null && mediaRecorder.state === 'recording';
        },

        // ── TTS: phát audio WAV từ .NET ───────────────────────────────────────

        playAudio: function (wavBytes) {
            try {
                // Dừng audio đang phát (nếu có)
                if (currentAudio) {
                    currentAudio.pause();
                    currentAudio = null;
                }

                const blob = new Blob([new Uint8Array(wavBytes)], { type: 'audio/wav' });
                const url = URL.createObjectURL(blob);

                currentAudio = new Audio(url);
                currentAudio.onended = () => {
                    URL.revokeObjectURL(url);
                    currentAudio = null;
                };
                currentAudio.onerror = (e) => {
                    console.error('[speechRecognition] Audio playback error:', e);
                    URL.revokeObjectURL(url);
                    currentAudio = null;
                };

                currentAudio.play().catch(err => {
                    console.error('[speechRecognition] play() failed:', err);
                });
            } catch (err) {
                console.error('[speechRecognition] playAudio failed:', err);
            }
        },

        stopAudio: function () {
            if (currentAudio) {
                currentAudio.pause();
                currentAudio = null;
            }
        },

        isSpeaking: function () {
            return currentAudio !== null && !currentAudio.paused;
        }
    };
})();
