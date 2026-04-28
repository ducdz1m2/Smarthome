using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Web.Services
{
    public class FileUploadService : IFileUploadService
    {
        private const int MaxFileSizeMB = 10;
        private const int BytesPerMB = 1024 * 1024;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return "";
            return $"{request.Scheme}://{request.Host}";
        }

        public async Task<string> UploadTempAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Chỉ chấp nhận file ảnh (jpg, png, gif, webp) và tài liệu (pdf, doc, docx)");

            if (file.Length > MaxFileSizeMB * BytesPerMB)
                throw new InvalidOperationException($"File quá lớn. Tối đa {MaxFileSizeMB}MB");

            var tempFolder = Path.Combine(_environment.WebRootPath, "uploads", "temp");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(tempFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Temp file uploaded: {FileName}", fileName);
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}/uploads/temp/{fileName}";
        }

        public async Task<string> UploadChatAsync(IFormFile file)
        {
            Console.WriteLine($"[FileUploadService] UploadChatAsync called - File: {file?.FileName}, Size: {file?.Length}");
            
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            Console.WriteLine($"[FileUploadService] File extension: {extension}");

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Chỉ chấp nhận file ảnh (jpg, png, gif, webp) và tài liệu (pdf, doc, docx)");

            if (file.Length > MaxFileSizeMB * BytesPerMB)
                throw new InvalidOperationException($"File quá lớn. Tối đa {MaxFileSizeMB}MB");

            var chatFolder = Path.Combine(_environment.WebRootPath, "uploads", "chat");
            Console.WriteLine($"[FileUploadService] Chat folder: {chatFolder}");
            if (!Directory.Exists(chatFolder))
            {
                Console.WriteLine($"[FileUploadService] Creating chat folder");
                Directory.CreateDirectory(chatFolder);
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(chatFolder, fileName);
            Console.WriteLine($"[FileUploadService] Saving file to: {filePath}");

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                Console.WriteLine($"[FileUploadService] File saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileUploadService] Error saving file: {ex.Message}");
                Console.WriteLine($"[FileUploadService] Stack trace: {ex.StackTrace}");
                throw;
            }

            _logger.LogInformation("Chat file uploaded: {FileName}", fileName);
            var baseUrl = GetBaseUrl();
            Console.WriteLine($"[FileUploadService] Base URL: {baseUrl}");
            var result = $"{baseUrl}/uploads/chat/{fileName}";
            Console.WriteLine($"[FileUploadService] Returning URL: {result}");
            return result;
        }

        public async Task<string> MoveToPermanentAsync(string tempPath, string folder)
        {
            if (string.IsNullOrWhiteSpace(tempPath))
                return tempPath;

            if (!tempPath.StartsWith("/uploads/temp/"))
                return tempPath; // Already permanent or external URL

            var fileName = Path.GetFileName(tempPath);
            var tempFullPath = Path.Combine(_environment.WebRootPath, "uploads", "temp", fileName);

            if (!File.Exists(tempFullPath))
                throw new FileNotFoundException("Temp file not found", tempFullPath);

            var permanentFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(permanentFolder))
                Directory.CreateDirectory(permanentFolder);

            var permanentPath = Path.Combine(permanentFolder, fileName);
            
            // If file exists in destination, generate new name
            if (File.Exists(permanentPath))
            {
                var extension = Path.GetExtension(fileName);
                var newFileName = $"{Guid.NewGuid():N}{extension}";
                permanentPath = Path.Combine(permanentFolder, newFileName);
                fileName = newFileName;
            }

            File.Move(tempFullPath, permanentPath);
            _logger.LogInformation("File moved from temp to {Folder}: {FileName}", folder, fileName);

            return $"{GetBaseUrl()}/uploads/{folder}/{fileName}";
        }

        public void DeleteTempFile(string tempPath)
        {
            if (string.IsNullOrWhiteSpace(tempPath) || !tempPath.StartsWith("/uploads/temp/"))
                return;

            try
            {
                var fileName = Path.GetFileName(tempPath);
                var fullPath = Path.Combine(_environment.WebRootPath, "uploads", "temp", fileName);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Temp file deleted: {FileName}", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp file: {TempPath}", tempPath);
            }
        }
    }
}
