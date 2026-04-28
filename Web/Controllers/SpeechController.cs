using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeechController : ControllerBase
{
    private readonly SpeechService _speechService;

    public SpeechController(SpeechService speechService)
    {
        _speechService = speechService;
    }

    [HttpPost("stt")]
    public async Task<IActionResult> Transcribe(IFormFile audio)
    {
        if (audio == null || audio.Length == 0)
        {
            return BadRequest(new { text = "" });
        }

        try
        {
            Console.WriteLine($"[SpeechController] Received audio: {audio.Length} bytes, contentType={audio.ContentType}");
            
            using var memoryStream = new MemoryStream();
            await audio.CopyToAsync(memoryStream);
            var audioBytes = memoryStream.ToArray();

            var text = await _speechService.TranscribeAsync(audioBytes, audio.ContentType ?? "audio/webm");
            
            Console.WriteLine($"[SpeechController] Transcription result: {text}");
            
            return Ok(new { text = text ?? "" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpeechController] Error: {ex.Message}");
            return Ok(new { text = "" });
        }
    }
}
