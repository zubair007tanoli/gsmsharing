using gsmsharing.ExeMethods;
using gsmsharing.Interfaces;
using gsmsharing.Repositories;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace gsmsharing.Controllers
{
    public class CommunityController : Controller
    {
        private ICommunityRepository communityRepository;
        private IUserService userService;

        public CommunityController(ICommunityRepository communityRepository, IUserService userService)
        {
            this.communityRepository = communityRepository;
            this.userService = userService;
        }
        [Authorize]
        [HttpPost]
        public IActionResult CreateCommunity(CommunityViewModel viewModel)
        {
            CommunityHelper.SetDefaultSeoValues(viewModel);
            string UID = userService.GetCurrentUserId()??string.Empty;
            communityRepository.CreateAsync(viewModel, UID);
            return RedirectToAction("Create","Posts");
        }
    }
}
