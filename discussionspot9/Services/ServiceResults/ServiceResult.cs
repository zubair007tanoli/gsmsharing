namespace discussionspot9.Services.ServiceResults
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public static ServiceResult SuccessResult() => new() { Success = true };
        public static ServiceResult ErrorResult(string message) => new() { Success = false, ErrorMessage = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> SuccessResult(T data) => new() { Success = true, Data = data };
    }

    public class CreateCommunityResult : ServiceResult
    {
        public string? Slug { get; set; }
    }

    public class CreatePostResult : ServiceResult
    {
        public int PostId { get; set; }
        public string? PostSlug { get; set; }
    }

    public class CreateCommentResult : ServiceResult
    {
        public int CommentId { get; set; }
    }

    public class VoteResult : ServiceResult
    {
        public int? UserVote { get; set; }
    }
}