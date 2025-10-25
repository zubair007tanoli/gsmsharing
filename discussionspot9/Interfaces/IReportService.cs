using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface IReportService
    {
        Task<ServiceResult> CreateReportAsync(int postId, string userId, string reason, string? details);
        Task<List<PostReportViewModel>> GetPendingReportsAsync();
        Task<List<PostReportViewModel>> GetAllReportsAsync(string status = "all", int page = 1, int pageSize = 20);
        Task<PostReportViewModel?> GetReportByIdAsync(int reportId);
        Task<ServiceResult> UpdateReportStatusAsync(int reportId, string status, string adminUserId, string? adminNotes = null);
        Task<int> GetPendingReportCountAsync();
        Task<ServiceResult> DismissReportAsync(int reportId, string adminUserId);
        Task<ServiceResult> ResolveReportAsync(int reportId, string adminUserId, string? action = null);
    }
}

