using Microsoft.AspNetCore.SignalR;

namespace GsmsharingV2.Hubs
{
    public class CommentHub : Hub
    {
        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}");
        }

        public async Task LeavePostGroup(int postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}");
        }

        public async Task SendComment(int postId, string content, int? parentCommentId, string userName)
        {
            await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", new
            {
                PostId = postId,
                Content = content,
                ParentCommentId = parentCommentId,
                UserName = userName,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}












