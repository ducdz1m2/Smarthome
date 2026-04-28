namespace Web.Services;

/// <summary>
/// Communicates with the Python speech service (speech-service/main.py).
/// STT: POST /stt  — multipart audio file → {"text": "...", "language": "...", "duration": ...}
/// TTS: POST /tts  — {"text": "...", "speed": 1.0} → audio/wav stream
/// </summary>
public class SpeechService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SpeechService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private string BaseUrl => _configuration["SpeechService:Url"] ?? "http://localhost:8003";

    /// <summary>
    /// Speech-to-text: gửi audio bytes lên /stt, trả về text đã nhận dạng.
    /// </summary>
    public async Task<string?> TranscribeAsync(byte[] audioData, string mimeType = "audio/webm")
    {
        if (audioData.Length == 0) return null;

        try
        {
            // Parse mime type để loại bỏ parameters (ví dụ: "audio/webm;codecs=opus" -> "audio/webm")
            var cleanMimeType = mimeType.Split(';')[0].Trim();
            Console.WriteLine($"[SpeechService] TranscribeAsync: {audioData.Length} bytes, mimeType={mimeType}, cleanMimeType={cleanMimeType}");
            
            using var httpClient = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();
            using var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(cleanMimeType);

            // Field name phải là "audio" theo speech-service/main.py
            var extension = cleanMimeType.Contains("wav") ? "recording.wav"
                          : cleanMimeType.Contains("ogg") ? "recording.ogg"
                          : "recording.webm";
            content.Add(audioContent, "audio", extension);

            Console.WriteLine($"[SpeechService] Sending to {BaseUrl}/stt");
            var response = await httpClient.PostAsync($"{BaseUrl}/stt", content);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SpeechService] STT failed {response.StatusCode}: {err}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[SpeechService] STT response: {json}");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("text").GetString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpeechService] TranscribeAsync error: {ex.Message}");
            Console.WriteLine($"[SpeechService] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Text-to-speech: gửi text lên /tts, trả về WAV bytes.
    /// </summary>
    public async Task<byte[]?> SynthesizeAsync(string text, float speed = 1.0f, int speakerId = 0)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            Console.WriteLine($"[SpeechService] SynthesizeAsync: '{text[..Math.Min(50, text.Length)]}...', speed={speed}, speakerId={speakerId}");
            using var httpClient = _httpClientFactory.CreateClient();
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                text,
                speed,
                speaker_id = speakerId
            });

            using var content = new StringContent(
                payload,
                System.Text.Encoding.UTF8,
                "application/json");

            Console.WriteLine($"[SpeechService] Sending to {BaseUrl}/tts");
            var response = await httpClient.PostAsync($"{BaseUrl}/tts", content);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SpeechService] TTS failed {response.StatusCode}: {err}");
                return null;
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"[SpeechService] TTS success: {audioBytes.Length} bytes");
            return audioBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpeechService] SynthesizeAsync error: {ex.Message}");
            Console.WriteLine($"[SpeechService] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Kiểm tra speech service có đang chạy không.
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            var response = await httpClient.GetAsync($"{BaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
