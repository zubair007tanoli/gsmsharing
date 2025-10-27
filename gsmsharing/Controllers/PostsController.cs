using gsmsharing.ExeMethods;
using gsmsharing.Interfaces;
using gsmsharing.Models;
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
        private readonly IPostRepository _postRepository;
        private readonly ISeo _seoRepository;
        private readonly IUserService _userService;
        public PostsController(ILogger<HomeController> logger, ICategoryRepository categoryRepository, IFileStorage fileStorage, ICommunityRepository communityRepository, IPostRepository postRepository, ISeo seoRepository, IUserService userService)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _fileStorage = fileStorage;
            _communityRepository = communityRepository;
            _postRepository = postRepository;
            _seoRepository = seoRepository;
            _userService = userService;
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
                _logger.LogInformation($"Uploading image: {postViewModel.Post.FeaturedImage.FileName}, Size: {postViewModel.Post.FeaturedImage.Length} bytes");
                
                var uploadResult = await _fileStorage.SaveImageAsync(
                    postViewModel.Post.FeaturedImage,
                    "posts/featured"
                );

                if (uploadResult.Succeeded)
                {
                    // Save the file path or URL to your post model
                    postViewModel.Post.FeaturedImagePath = uploadResult.FilePath;
                    postViewModel.Post.FeaturedImageUrl = uploadResult.FileUrl;
                    
                    _logger.LogInformation($"Image uploaded successfully:");
                    _logger.LogInformation($"  - FilePath: {uploadResult.FilePath}");
                    _logger.LogInformation($"  - FileUrl: {uploadResult.FileUrl}");
                    _logger.LogInformation($"  - Size: {uploadResult.FileSize} bytes");
                }
                else
                {
                    _logger.LogError($"Image upload failed: {uploadResult.ErrorMessage}");
                    ModelState.AddModelError("Post.FeaturedImage", uploadResult.ErrorMessage);
                    return View(postViewModel);
                }
            }
            else
            {
                _logger.LogWarning("No featured image provided for post creation");
            }

            var ModelPost = ViewModelExtensions.ToModel(postViewModel.Post);
            ModelPost.UserId =  _userService?.GetCurrentUserId() ?? "Anonymous";
            
            _logger.LogInformation($"Saving post to database - FeaturedImage value: '{ModelPost.FeaturedImage}'");
            
            var data = await _postRepository.CreateAsync(ModelPost);
            ModelPost.PostID = (int)data;
            
            _logger.LogInformation($"Post created successfully with ID: {ModelPost.PostID}");
            
            await _seoRepository.GenerateAndSaveSchema(ModelPost,ModelPost.UserId);
            return RedirectToAction("Details", new { community = ModelPost.Community?.Name ?? "gsmsharing", slug = ModelPost.Slug });
        }


        [HttpGet]
        public async Task<IActionResult> GetPost(int id)
        {
            var resp = await _postRepository.GetByIdAsync(14);
            return View(resp);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string community, string slug)
        {
            if (string.IsNullOrEmpty(community) || string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _postRepository.GetBySlugAndCommunityAsync(slug, community);
            
            if (post == null)
            {
                _logger.LogWarning($"Post not found: Community={community}, Slug={slug}");
                return NotFound();
            }

            // Debug logging for image path
            _logger.LogInformation($"Post {post.PostID} - FeaturedImage from DB: '{post.FeaturedImage}'");

            // Increment view count (implement this method in repository)
            // await _postRepository.IncrementViewCountAsync(post.PostID);

            return View("GetPost", post);
        }

        public IActionResult Post()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommunity(CreatePostAndCommunityViewModel postViewModel)
        {
            return Redirect("Create");
        }
    }
}
