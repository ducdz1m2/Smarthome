using Microsoft.JSInterop;

namespace Web.Services;

/// <summary>
/// Quản lý ghi âm từ microphone (qua JS interop) và gửi audio lên SpeechService để nhận dạng.
/// Cũng hỗ trợ TTS: gọi SpeechService.SynthesizeAsync rồi phát qua JS.
/// </summary>
public class SpeechRecognitionService : IAsyncDisposable
{
    private readonly SpeechService _speechService;
    private readonly IJSRuntime _jsRuntime;

    private bool _isRecording = false;
    private Func<string, Task>? _onTranscribed;
    private DotNetObjectReference<SpeechRecognitionService>? _dotNetRef;

    public SpeechRecognitionService(SpeechService speechService, IJSRuntime jsRuntime)
    {
        _speechService = speechService;
        _jsRuntime = jsRuntime;
    }

    public bool IsRecording => _isRecording;

    // ── STT ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Bắt đầu ghi âm. Khi dừng, audio được gửi lên /stt và kết quả trả về qua callback.
    /// </summary>
    public async Task StartRecordingAsync(Func<string, Task> onTranscribed)
    {
        if (_isRecording) return;

        _onTranscribed = onTranscribed;
        _dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            var started = await _jsRuntime.InvokeAsync<bool>(
                "speechRecognition.startRecording", _dotNetRef);

            if (started)
            {
                _isRecording = true;
            }
            else
            {
                _onTranscribed = null;
                _dotNetRef?.Dispose();
                _dotNetRef = null;
                throw new InvalidOperationException(
                    "Không thể truy cập microphone. Vui lòng kiểm tra quyền truy cập.");
            }
        }
        catch (JSException ex)
        {
            _onTranscribed = null;
            _dotNetRef?.Dispose();
            _dotNetRef = null;
            throw new InvalidOperationException($"Lỗi khởi động ghi âm: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Dừng ghi âm và chờ transcription. Dùng cho flow pull-based (Products.razor).
    /// Kết quả thực sự được trả về qua callback đã đăng ký trong StartRecordingAsync.
    /// </summary>
    public async Task<string?> StopAndTranscribeAsync()
    {
        if (!_isRecording) return null;

        try
        {
            await _jsRuntime.InvokeVoidAsync("speechRecognition.stopRecording");
        }
        catch (JSException ex)
        {
            Console.WriteLine($"[SpeechRecognitionService] stopRecording JS error: {ex.Message}");
        }

        _isRecording = false;
        return null; // Kết quả được trả về qua OnAudioCaptured → callback
    }

    /// <summary>
    /// Hủy ghi âm, không transcribe.
    /// </summary>
    public async Task CancelRecordingAsync()
    {
        if (!_isRecording) return;

        _isRecording = false;
        _onTranscribed = null;

        try
        {
            await _jsRuntime.InvokeVoidAsync("speechRecognition.cancelRecording");
        }
        catch
        {
            // Bỏ qua lỗi JS khi circuit đang đóng
        }
        finally
        {
            _dotNetRef?.Dispose();
            _dotNetRef = null;
        }
    }

    /// <summary>
    /// Được gọi từ JavaScript khi transcription hoàn tất (sau khi gửi audio qua HTTP).
    /// </summary>
    [JSInvokable]
    public async Task OnTranscriptionResult(string text)
    {
        Console.WriteLine($"[SpeechRecognitionService] OnTranscriptionResult: '{text}'");
        _isRecording = false;

        var callback = _onTranscribed;
        _onTranscribed = null;
        _dotNetRef?.Dispose();
        _dotNetRef = null;

        if (callback == null)
        {
            Console.WriteLine("[SpeechRecognitionService] OnTranscriptionResult: no callback");
            return;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine($"[SpeechRecognitionService] Invoking callback with text: {text}");
                await callback(text);
            }
            else
            {
                Console.WriteLine("[SpeechRecognitionService] Transcription result is empty");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpeechRecognitionService] Callback error: {ex.Message}");
        }
    }

    // ── TTS ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Chuyển text thành giọng nói và phát qua trình duyệt.
    /// </summary>
    public async Task SpeakAsync(string text, float speed = 1.0f, int speakerId = 0)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            Console.WriteLine($"[SpeechRecognitionService] SpeakAsync: '{text[..Math.Min(50, text.Length)]}...', speed={speed}, speakerId={speakerId}");
            var wavBytes = await _speechService.SynthesizeAsync(text, speed, speakerId);

            if (wavBytes == null || wavBytes.Length == 0)
            {
                Console.WriteLine("[SpeechRecognitionService] TTS returned empty audio");
                return;
            }

            Console.WriteLine($"[SpeechRecognitionService] TTS audio received: {wavBytes.Length} bytes, sending to JS for playback");
            // Phát audio qua JS
            await _jsRuntime.InvokeVoidAsync("speechRecognition.playAudio", wavBytes);
            Console.WriteLine("[SpeechRecognitionService] Audio playback initiated via JS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpeechRecognitionService] SpeakAsync error: {ex.Message}");
            Console.WriteLine($"[SpeechRecognitionService] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Dừng phát TTS đang chạy.
    /// </summary>
    public async Task StopSpeakingAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("speechRecognition.stopAudio");
        }
        catch { }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isRecording)
        {
            try { await _jsRuntime.InvokeVoidAsync("speechRecognition.cancelRecording"); }
            catch { }
        }
        _dotNetRef?.Dispose();
    }
}
