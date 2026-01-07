using Microsoft.AspNetCore.SignalR;

namespace GsmsharingV2.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        public async Task SendNotification(string userId, string title, string content, string type)
        {
            await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                Title = title,
                Content = content,
                Type = type,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}

























