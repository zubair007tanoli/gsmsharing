namespace gsmsharing.Interfaces
{
    public interface IGptService
    {
        Task<string> GenerateContentAsync(string title);
    }
}
