using System.Threading.Tasks;

namespace Fluxdox.Services
{
    public interface IStorageService
    {
        Task<string> GetPresignedUploadUrlAsync(string key, int expiresInSeconds = 3600);
        Task<string> GetPresignedDownloadUrlAsync(string key, int expiresInSeconds = 3600);
    }
}