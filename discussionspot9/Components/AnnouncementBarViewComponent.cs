using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace discussionspot9.Components
{
    public class AnnouncementBarViewComponent : ViewComponent
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public AnnouncementBarViewComponent(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var announcements = await _announcementRepository.GetActiveAnnouncementsAsync();
            return View(announcements);
        }
    }
}

