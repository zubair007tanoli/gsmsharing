using System.Threading.Tasks;

namespace Fluxdox.Services
{
    public class StubStorageService : IStorageService
    {
        public Task<string> GetPresignedDownloadUrlAsync(string key, int expiresInSeconds = 3600)
        {
            // In real implementation generate presigned URL for S3
            return Task.FromResult($"/downloads/{key}");
        }

        public Task<string> GetPresignedUploadUrlAsync(string key, int expiresInSeconds = 3600)
        {
            // Return a fake presigned URL for development
            return Task.FromResult($"/uploads/{key}");
        }
    }
}