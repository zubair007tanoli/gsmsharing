using Microsoft.AspNetCore.SignalR;

namespace FluxPDF.Hubs
{
    public class ProcessingHub : Hub
    {
        public async Task SendProgressUpdate(string jobId, int progress, string message) { await Clients.Group(jobId).SendAsync("ReceiveProgressUpdate", jobId, progress, message); }
        public async Task JoinJobGroup(string jobId) { await Groups.AddToGroupAsync(Context.ConnectionId, jobId); }
        public async Task LeaveJobGroup(string jobId) { await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId); }
    }
}
