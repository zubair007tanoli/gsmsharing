using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace discussionspot9.Hubs
{
    public class PostHub : Hub
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;

        public PostHub(IPostService postService, ICommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }

        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}");
        }

        public async Task LeavePostGroup(int postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}");
        }

        [Authorize]
        public async Task SendComment(int postId, string content, int? parentCommentId)
        {
            var userId = Context.UserIdentifier;
            var comment = await _commentService.CreateCommentAsync(new CreateCommentViewModel
            {
                PostId = postId,
                Content = content,
                ParentCommentId = parentCommentId,
                UserId = userId
            });

            if (comment.Success)
            {
                var commentData = await _commentService.GetCommentByIdAsync(comment.CommentId);
                await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", commentData);
            }
        }

        public async Task UpdateVoteCount(int postId, int newCount)
        {
            await Clients.Group($"post-{postId}").SendAsync("UpdateVoteCount", newCount);
        }
    }
}