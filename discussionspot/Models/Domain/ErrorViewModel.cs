namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents error information for the error view
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ErrorMessage { get; set; }

        public int StatusCode { get; set; }
    }
}
