namespace gsmsharing.Models.ImageModels
{
    public class FileStorageOptions
    {
        public string RootDirectory { get; set; } = "wwwroot/uploads";
        public long MaxFileSizeInBytes { get; set; } = 5 * 1024 * 1024; // 5MB default
        public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };
        public string[] AllowedMimeTypes { get; set; } =
        {
            "image/jpeg",
            "image/png",
            "image/gif"
        };
        public bool PreserveFileName { get; set; } = false;
        public string BaseUrl { get; set; } = "/uploads";
    }
}
