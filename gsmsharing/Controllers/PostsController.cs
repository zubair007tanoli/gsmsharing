using gsmsharing.ExeMethods;
using gsmsharing.Interfaces;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Controllers
{
    public class PostsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileStorage _fileStorage;
        private readonly ICommunityRepository _communityRepository;

        public PostsController(ILogger<HomeController> logger, ICategoryRepository categoryRepository, IFileStorage fileStorage, ICommunityRepository communityRepository)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _fileStorage = fileStorage;
            _communityRepository = communityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            
            var viewModel = new CreatePostAndCommunityViewModel()
            {
                Post = new PostViewModel(),
                Community = new CommunityViewModel()
            };
            viewModel.Post.Communities = await _communityRepository.GetCommunitiesForDropdownAsync();
            viewModel.Community.Categories = await _categoryRepository.CreateCategorySelectListAsync();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePostAndCommunityViewModel postViewModel)
        {
            PostViewModelExtensions.SetDefaultSeoValues(postViewModel.Post);
            if (postViewModel.Post.FeaturedImage != null)
            {
                var uploadResult = await _fileStorage.SaveImageAsync(
                    postViewModel.Post.FeaturedImage,
                    "posts/featured"
                );

                if (uploadResult.Succeeded)
                {
                    // Save the file path or URL to your post model
                    postViewModel.Post.FeaturedImagePath = uploadResult.FilePath;
                    postViewModel.Post.FeaturedImageUrl = uploadResult.FileUrl;
                }
                else
                {
                    ModelState.AddModelError("Post.FeaturedImage", uploadResult.ErrorMessage);
                    return View(postViewModel);
                }
               
            }
            return RedirectToAction("Create");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommunity(CreatePostAndCommunityViewModel postViewModel)
        {
            return Redirect("Create");
        }
    }
}
