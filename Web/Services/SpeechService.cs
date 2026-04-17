using Microsoft.JSInterop;

namespace Web.Services;

public class SpeechService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IConfiguration _configuration;

    public SpeechService(IJSRuntime jsRuntime, IConfiguration configuration)
    {
        _jsRuntime = jsRuntime;
        _configuration = configuration;
    }

    private string SpeechServiceUrl => _configuration["SpeechService:Url"] ?? "http://localhost:8003";

    /// <summary>
    /// Chuyển văn bản thành âm thanh và phát trực tiếp
    /// </summary>
    public async Task SpeakAsync(string text, double speed = 1.0)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("speakText", SpeechServiceUrl, text, speed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TTS error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra health của speech service
    /// </summary>
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<bool>("checkSpeechServiceHealth", SpeechServiceUrl);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Speech service health check failed: {ex.Message}");
            return false;
        }
    }
}
