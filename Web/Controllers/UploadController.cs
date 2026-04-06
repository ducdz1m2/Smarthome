using Web.Services;
using Microsoft.AspNetCore.Mvc;

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
    }
}
