using Microsoft.AspNetCore.Http;

namespace Web.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadTempAsync(IFormFile file);
        Task<string> MoveToPermanentAsync(string tempPath, string folder);
        void DeleteTempFile(string tempPath);
    }
}
