namespace gsmsharing.Models.ImageModels
{
    public class FileUploadResult
    {
        public bool Succeeded { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public string ErrorMessage { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public static FileUploadResult Success(string filePath, string fileUrl, 
            long fileSize, string contentType) =>
            new FileUploadResult
            {
                Succeeded = true,
                FilePath = filePath,
                FileUrl = fileUrl,
                FileSize = fileSize,
                ContentType = contentType
            };

            public static FileUploadResult Failure(string errorMessage) =>
            new FileUploadResult
            {
                Succeeded = false,
                ErrorMessage = errorMessage
            };
    }
}
