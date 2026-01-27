namespace GsmsharingV2.Interfaces
{
    public interface IAIService
    {
        Task<string> GeneratePostTitleAsync(string topic, string context);
        Task<string> GeneratePostContentAsync(string title, string topic, int maxLength = 1000);
        Task<string> GenerateExcerptAsync(string content, int maxLength = 200);
        Task<string> GenerateTagsAsync(string content, int maxTags = 5);
        Task<string> ImproveContentAsync(string content);
        Task<string> SummarizeContentAsync(string content, int maxLength = 300);
        Task<bool> ModerateContentAsync(string content);
    }
}
