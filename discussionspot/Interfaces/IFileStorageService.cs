namespace discussionspot.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string path);
        Task<byte[]> GetFileAsync(string path);
        Task<string> GetFileUrlAsync(string path);
        string GetRelativePath(string path);
        Task<string> UploadBase64ImageAsync(string base64String, string folder);
        Task<bool> FileExistsAsync(string path);
    }
}
