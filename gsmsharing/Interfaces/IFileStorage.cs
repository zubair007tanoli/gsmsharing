using gsmsharing.Models.ImageModels;

namespace gsmsharing.Interfaces
{
    public interface IFileStorage 
    {
        Task<FileUploadResult> SaveImageAsync(IFormFile file, string directory = "images");
        Task<FileUploadResult> SaveImageAsync(byte[] fileBytes, string fileName, string directory = "images");
        Task<bool> DeleteImageAsync(string filePath);
        Task<byte[]> GetImageAsync(string filePath);
        bool ValidateImage(IFormFile file);
        string GetFileUrl(string filePath);
    }
}
