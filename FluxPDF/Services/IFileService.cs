namespace FluxPDF.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string userId);
        Task<bool> DeleteFileAsync(string filePath);
        Task<Stream> GetFileStreamAsync(string filePath);
        bool ValidateFile(IFormFile file);
        string GetFileExtension(string fileName);
    }
}
