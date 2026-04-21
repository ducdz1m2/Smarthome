// Speech Recognition & TTS — Audio capture + playback
window.speechRecognition = (function () {
    let mediaRecorder = null;
    let audioChunks = [];
    let dotNetRef = null;
    let currentAudio = null; // for TTS playback

    return {
        // ── STT: ghi âm từ microphone ─────────────────────────────────────────

        startRecording: async function (dotNetReference) {
            try {
                dotNetRef = dotNetReference;
                audioChunks = [];

                const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

                // Ưu tiên webm/opus, fallback sang bất kỳ format nào được hỗ trợ
                const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
                    ? 'audio/webm;codecs=opus'
                    : MediaRecorder.isTypeSupported('audio/webm')
                        ? 'audio/webm'
                        : '';

                mediaRecorder = mimeType
                    ? new MediaRecorder(stream, { mimeType })
                    : new MediaRecorder(stream);

                mediaRecorder.ondataavailable = (event) => {
                    if (event.data.size > 0) {
                        audioChunks.push(event.data);
                    }
                };

                mediaRecorder.onstop = async () => {
                    // Giải phóng microphone
                    stream.getTracks().forEach(track => track.stop());

                    if (!dotNetRef) return; // đã bị cancel

                    const audioBlob = new Blob(audioChunks, {
                        type: mediaRecorder.mimeType || 'audio/webm'
                    });
                    const arrayBuffer = await audioBlob.arrayBuffer();
                    const uint8Array = new Uint8Array(arrayBuffer);

                    // Gửi audio bytes về .NET
                    await dotNetRef.invokeMethodAsync(
                        'OnAudioCaptured',
                        Array.from(uint8Array),
                        mediaRecorder.mimeType || 'audio/webm'
                    );
                    dotNetRef = null;
                };

                mediaRecorder.start();
                return true;
            } catch (err) {
                console.error('[speechRecognition] startRecording failed:', err);
                dotNetRef = null;
                return false;
            }
        },

        stopRecording: function () {
            if (mediaRecorder && mediaRecorder.state === 'recording') {
                mediaRecorder.stop();
                return true;
            }
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
