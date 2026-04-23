using Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IFileUploadService fileUploadService, ILogger<UploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [HttpPost("temp")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadTemp(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "Không có file nào được chọn" });

                var tempPath = await _fileUploadService.UploadTempAsync(file);
                
                return Ok(new { 
                    tempPath = tempPath,
                    url = tempPath,
                    fileName = file.FileName,
                    size = file.Length
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = "Lỗi khi upload file" });
            }
        }
[AllowAnonymous]
        
        [HttpDelete("temp")]
        public IActionResult DeleteTemp([FromQuery] string path)
        {
            try
            {
                _fileUploadService.DeleteTempFile(path);
                return Ok(new { message = "Đã xóa file tạm" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting temp file");
                return StatusCode(500, new { error = "Lỗi khi xóa file" });
            }
        }

        [HttpPost("chat")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadChat(IFormFile file)
        {
            try
            {
                Console.WriteLine($"[UploadController] UploadChat called - File: {file?.FileName}, Size: {file?.Length}");
                
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "Không có file nào được chọn" });

                var chatPath = await _fileUploadService.UploadChatAsync(file);
                Console.WriteLine($"[UploadController] File uploaded successfully - Path: {chatPath}");
                
                return Ok(new { 
                    url = chatPath,
                    fileName = file.FileName,
                    size = file.Length
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[UploadController] InvalidOperationException: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadController] Exception: {ex.Message}");
                _logger.LogError(ex, "Error uploading chat file");
                return StatusCode(500, new { error = "Lỗi khi upload file" });
            }
        }
    }
}
