using Microsoft.JSInterop;

namespace Web.Services;

public class SpeechRecognitionService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IConfiguration _configuration;

    public SpeechRecognitionService(IJSRuntime jsRuntime, IConfiguration configuration)
    {
        _jsRuntime = jsRuntime;
        _configuration = configuration;
    }

    private string SpeechServiceUrl => _configuration["SpeechService:Url"] ?? "http://localhost:8003";

    public bool IsRecording { get; private set; }
    public bool IsTranscribing { get; private set; }

    /// <summary>
    /// Bắt đầu ghi âm từ micro với VAD
    /// </summary>
    public async Task StartRecordingAsync(Action<string> onTranscriptionComplete)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("startRecording", SpeechServiceUrl, DotNetObjectReference.Create(new JsCallback(this, onTranscriptionComplete)));
            IsRecording = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Start recording error: {ex.Message}");
            throw new Exception("Không thể truy cập micro. Vui lòng kiểm tra quyền truy cập.");
        }
    }

    /// <summary>
    /// Dừng ghi âm và chuyển đổi thành văn bản
    /// </summary>
    public async Task<string> StopAndTranscribeAsync()
    {
        try
        {
            IsRecording = false;
            IsTranscribing = true;
            var text = await _jsRuntime.InvokeAsync<string>("stopAndTranscribe");
            IsTranscribing = false;
            return text ?? "";
        }
        catch (Exception ex)
        {
            IsTranscribing = false;
            Console.WriteLine($"Stop recording error: {ex.Message}");
            throw new Exception("Lỗi nhận dạng giọng nói. Vui lòng thử lại.");
        }
    }

    /// <summary>
    /// Hủy ghi âm
    /// </summary>
    public async Task CancelRecordingAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("cancelRecording");
            IsRecording = false;
            // Note: IsTranscribing will be reset by the callback
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cancel recording error: {ex.Message}");
        }
    }

    /// <summary>
    /// Callback class for JavaScript interop
    /// </summary>
    private class JsCallback
    {
        private readonly SpeechRecognitionService _service;
        private readonly Action<string> _onTranscriptionComplete;

        public JsCallback(SpeechRecognitionService service, Action<string> onTranscriptionComplete)
        {
            _service = service;
            _onTranscriptionComplete = onTranscriptionComplete;
        }

        [JSInvokable]
        public void OnTranscriptionComplete(string text)
        {
            _service.IsRecording = false;
            _service.IsTranscribing = false;
            _onTranscriptionComplete?.Invoke(text);
        }
    }
}
