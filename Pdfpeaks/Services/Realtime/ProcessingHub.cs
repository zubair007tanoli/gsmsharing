using Microsoft.AspNetCore.SignalR;
using Pdfpeaks.Models;

namespace Pdfpeaks.Services.Realtime;

/// <summary>
/// SignalR hub for real-time processing updates
/// </summary>
public class ProcessingHub : Hub
{
    private readonly ILogger<ProcessingHub> _logger;

    public ProcessingHub(ILogger<ProcessingHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a processing room for specific file
    /// </summary>
    public async Task JoinProcessingRoom(string fileId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"processing:{fileId}");
        _logger.LogInformation("Client {ConnectionId} joined processing room: {FileId}", Context.ConnectionId, fileId);
    }

    /// <summary>
    /// Leave processing room
    /// </summary>
    public async Task LeaveProcessingRoom(string fileId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"processing:{fileId}");
        _logger.LogInformation("Client {ConnectionId} left processing room: {FileId}", Context.ConnectionId, fileId);
    }

    /// <summary>
    /// Join user-specific notification room
    /// </summary>
    public async Task JoinUserRoom(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        _logger.LogInformation("Client {ConnectionId} joined user room: {UserId}", Context.ConnectionId, userId);
    }

    /// <summary>
    /// Subscribe to global notifications
    /// </summary>
    public async Task SubscribeToUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "global");
        _logger.LogInformation("Client {ConnectionId} subscribed to global updates", Context.ConnectionId);
    }
}

/// <summary>
/// Processing update message
/// </summary>
public class ProcessingUpdate
{
    public string FileId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// SignalR broadcast service
/// </summary>
public class ProcessingBroadcastService
{
    private readonly IHubContext<ProcessingHub> _hubContext;
    private readonly ILogger<ProcessingBroadcastService> _logger;

    public ProcessingBroadcastService(
        IHubContext<ProcessingHub> hubContext,
        ILogger<ProcessingBroadcastService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Broadcast processing update to specific file room
    /// </summary>
    public async Task BroadcastUpdateAsync(string fileId, ProcessingUpdate update)
    {
        _logger.LogDebug("Broadcasting update for file {FileId}: {Status}", fileId, update.Status);
        
        await _hubContext.Clients.Group($"processing:{fileId}")
            .SendAsync("ProcessingUpdate", update);
    }

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task NotifyUserAsync(string userId, string type, object data)
    {
        _logger.LogDebug("Notifying user {UserId}: {Type}", userId, type);
        
        await _hubContext.Clients.Group($"user:{userId}")
            .SendAsync("Notification", new
            {
                Type = type,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcast global announcement
    /// </summary>
    public async Task BroadcastGlobalAsync(string title, string message, string type = "info")
    {
        await _hubContext.Clients.Group("global")
            .SendAsync("GlobalAnnouncement", new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcast processing started
    /// </summary>
    public async Task ProcessingStartedAsync(string fileId, string operationType)
    {
        await BroadcastUpdateAsync(fileId, new ProcessingUpdate
        {
            FileId = fileId,
            OperationType = operationType,
            Status = "started",
            Progress = 0,
            Message = $"Processing {operationType} started"
        });
    }

    /// <summary>
    /// Broadcast processing progress
    /// </summary>
    public async Task ProcessingProgressAsync(string fileId, int progress, string message)
    {
        await BroadcastUpdateAsync(fileId, new ProcessingUpdate
        {
            FileId = fileId,
            Status = "progress",
            Progress = progress,
            Message = message
        });
    }

    /// <summary>
    /// Broadcast processing completed
    /// </summary>
    public async Task ProcessingCompletedAsync(string fileId, string downloadUrl, Dictionary<string, object>? metadata = null)
    {
        await BroadcastUpdateAsync(fileId, new ProcessingUpdate
        {
            FileId = fileId,
            Status = "completed",
            Progress = 100,
            Message = "Processing completed successfully",
            Metadata = metadata
        });

        // Also notify the user
        if (metadata?.TryGetValue("UserId", out var userId) == true)
        {
            await NotifyUserAsync(userId.ToString()!, "processing_completed", new
            {
                FileId = fileId,
                DownloadUrl = downloadUrl,
                Metadata = metadata
            });
        }
    }

    /// <summary>
    /// Broadcast processing error
    /// </summary>
    public async Task ProcessingErrorAsync(string fileId, string errorMessage)
    {
        await BroadcastUpdateAsync(fileId, new ProcessingUpdate
        {
            FileId = fileId,
            Status = "error",
            Progress = 0,
            Message = errorMessage
        });
    }
}
