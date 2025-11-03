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
            _logger.LogInformation("=== POST CREATION STARTED ===");
            _logger.LogInformation($"Post Title: {postViewModel.Post?.Title}");
            _logger.LogInformation($"Has FeaturedImage: {postViewModel.Post?.FeaturedImage != null}");
            
            PostViewModelExtensions.SetDefaultSeoValues(postViewModel.Post);
            if (postViewModel.Post.FeaturedImage != null)
            {
                _logger.LogInformation($"📸 Uploading image: {postViewModel.Post.FeaturedImage.FileName}, Size: {postViewModel.Post.FeaturedImage.Length} bytes");
                
                var uploadResult = await _fileStorage.SaveImageAsync(
                    postViewModel.Post.FeaturedImage,
                    "posts/featured"
                );

                if (uploadResult.Succeeded)
                {
                    // Save the file path or URL to your post model
                    postViewModel.Post.FeaturedImagePath = uploadResult.FilePath;
                    postViewModel.Post.FeaturedImageUrl = uploadResult.FileUrl;
                    
                    _logger.LogInformation($"✅ Image uploaded successfully:");
                    _logger.LogInformation($"  - FilePath: {uploadResult.FilePath}");
                    _logger.LogInformation($"  - FileUrl: {uploadResult.FileUrl}");
                    _logger.LogInformation($"  - Size: {uploadResult.FileSize} bytes");
                    _logger.LogInformation($"  - ContentType: {uploadResult.ContentType}");
                }
                else
                {
                    _logger.LogError($"❌ Image upload failed: {uploadResult.ErrorMessage}");
                    ModelState.AddModelError("Post.FeaturedImage", uploadResult.ErrorMessage);
                    return View(postViewModel);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ No featured image provided for post creation");
            }

            var ModelPost = ViewModelExtensions.ToModel(postViewModel.Post);
            ModelPost.UserId =  _userService?.GetCurrentUserId() ?? "Anonymous";
            
            _logger.LogInformation($"💾 Saving post to database:");
            _logger.LogInformation($"  - Title: {ModelPost.Title}");
            _logger.LogInformation($"  - Slug: {ModelPost.Slug}");
            _logger.LogInformation($"  - FeaturedImage: '{ModelPost.FeaturedImage ?? "NULL"}'");
            _logger.LogInformation($"  - UserId: {ModelPost.UserId}");
            
            var data = await _postRepository.CreateAsync(ModelPost);
            ModelPost.PostID = (int)data;
            
            _logger.LogInformation($"✅ Post created successfully with ID: {ModelPost.PostID}");
            _logger.LogInformation($"=== POST CREATION COMPLETED ===");
            
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

        // ============================================
        // EDIT POST FUNCTIONALITY
        // ============================================
        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation($"=== EDIT POST GET - PostID: {id} ===");
            
            var currentUserId = _userService?.GetCurrentUserId();
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("User not authenticated - redirecting to login");
                return RedirectToAction("Login", "UserAccount");
            }

            // Get the post with all details
            var postData = await _postRepository.GetByIdAsync(id);
            
            if (postData == null)
            {
                _logger.LogWarning($"Post not found: {id}");
                return NotFound();
            }

            // Check ownership
            if (postData.UserId != currentUserId)
            {
                _logger.LogWarning($"User {currentUserId} attempted to edit post {id} owned by {postData.UserId}");
                return Forbid();
            }

            // Convert to Post model and then to EditViewModel
            var post = new Post
            {
                PostID = postData.PostID,
                UserId = postData.UserId,
                Title = postData.Title,
                Slug = postData.Slug,
                Tags = postData.Tags,
                Content = postData.Content,
                FeaturedImage = postData.FeaturedImage,
                CommunityID = postData.CommunityID,
                AllowComments = postData.AllowComments,
                IsPromoted = postData.IsPromoted,
                IsFeatured = postData.IsFeatured,
                MetaTitle = postData.MetaTitle,
                MetaDescription = postData.MetaDescription,
                OgTitle = postData.OgTitle,
                OgDescription = postData.OgDescription,
                PostStatus = postData.PostStatus
            };

            var viewModel = post.ToEditViewModel();
            viewModel.Communities = await _communityRepository.GetCommunitiesForDropdownAsync();

            _logger.LogInformation($"✅ Loaded post for editing: {viewModel.Title}");
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel viewModel)
        {
            _logger.LogInformation($"=== EDIT POST POST - PostID: {viewModel.PostID} ===");
            
            var currentUserId = _userService?.GetCurrentUserId();
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "UserAccount");
            }

            // Verify ownership again
            var existingPost = await _postRepository.GetByIdAsync(viewModel.PostID);
            if (existingPost == null)
            {
                return NotFound();
            }

            if (existingPost.UserId != currentUserId)
            {
                _logger.LogWarning($"User {currentUserId} attempted to edit post {viewModel.PostID} owned by {existingPost.UserId}");
                return Forbid();
            }

            viewModel.SetDefaultSeoValues();

            // Handle image upload if user wants to replace
            if (viewModel.ImageAction == "replace" && viewModel.FeaturedImage != null)
            {
                _logger.LogInformation($"📸 Replacing image: {viewModel.FeaturedImage.FileName}");
                
                var uploadResult = await _fileStorage.SaveImageAsync(
                    viewModel.FeaturedImage,
                    "posts/featured"
                );

                if (uploadResult.Succeeded)
                {
                    viewModel.FeaturedImageUrl = uploadResult.FileUrl;
                    _logger.LogInformation($"✅ New image uploaded: {uploadResult.FileUrl}");
                    
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingPost.FeaturedImage))
                    {
                        await _fileStorage.DeleteImageAsync(existingPost.FeaturedImage);
                        _logger.LogInformation($"🗑️ Old image deleted: {existingPost.FeaturedImage}");
                    }
                }
                else
                {
                    _logger.LogError($"❌ Image upload failed: {uploadResult.ErrorMessage}");
                    ModelState.AddModelError("FeaturedImage", uploadResult.ErrorMessage);
                    viewModel.Communities = await _communityRepository.GetCommunitiesForDropdownAsync();
                    return View(viewModel);
                }
            }

            // Convert to Post model
            var post = viewModel.ToModelFromEdit(existingPost.FeaturedImage);
            post.UserId = currentUserId;

            _logger.LogInformation($"💾 Updating post:");
            _logger.LogInformation($"  - Title: {post.Title}");
            _logger.LogInformation($"  - Slug: {post.Slug} (unchanged)");
            _logger.LogInformation($"  - FeaturedImage: {post.FeaturedImage ?? "NULL"}");
            _logger.LogInformation($"  - PostStatus: {post.PostStatus}");
            _logger.LogInformation($"  - ImageAction: {viewModel.ImageAction}");

            var updatedPost = await _postRepository.UpdateAsync(post);

            if (updatedPost != null)
            {
                _logger.LogInformation($"✅ Post updated successfully: {post.PostID}");
                
                // Update SEO if needed
                await _seoRepository.GenerateAndSaveSchema(post, currentUserId);
                
                // Get community info for redirect
                var postWithCommunity = await _postRepository.GetByIdAsync(post.PostID);
                return RedirectToAction("Details", new { 
                    community = postWithCommunity.CommunityName ?? "gsmsharing", 
                    slug = post.Slug 
                });
            }
            else
            {
                _logger.LogError("Failed to update post");
                ModelState.AddModelError("", "Failed to update post");
                viewModel.Communities = await _communityRepository.GetCommunitiesForDropdownAsync();
                return View(viewModel);
            }
        }

        // ============================================
        // DELETE POST FUNCTIONALITY
        // ============================================
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"=== DELETE POST - PostID: {id} ===");
            
            var currentUserId = _userService?.GetCurrentUserId();
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            // Get the post
            var post = await _postRepository.GetByIdAsync(id);
            
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Check ownership
            if (post.UserId != currentUserId)
            {
                _logger.LogWarning($"User {currentUserId} attempted to delete post {id} owned by {post.UserId}");
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Delete the image if it exists
            if (!string.IsNullOrEmpty(post.FeaturedImage))
            {
                await _fileStorage.DeleteImageAsync(post.FeaturedImage);
                _logger.LogInformation($"🗑️ Image deleted: {post.FeaturedImage}");
            }

            // Delete the post
            await _postRepository.DeleteAsync(id);
            
            _logger.LogInformation($"✅ Post deleted successfully: {id}");
            
            return Json(new { success = true, message = "Post deleted successfully" });
        }

        // ============================================
        // MY POSTS - USER PROFILE
        // ============================================
        
        [HttpGet]
        public async Task<IActionResult> MyPosts(string status = "all")
        {
            var currentUserId = _userService?.GetCurrentUserId();
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "UserAccount");
            }

            _logger.LogInformation($"=== MY POSTS - User: {currentUserId}, Status: {status} ===");

            var userPosts = await _postRepository.GetByUserIdAsync(currentUserId);

            // Filter by status if specified
            if (status != "all")
            {
                userPosts = userPosts.Where(p => p.PostStatus?.ToLower() == status.ToLower());
            }

            ViewBag.CurrentFilter = status;
            ViewBag.DraftCount = userPosts.Count(p => p.PostStatus?.ToLower() == "draft");
            ViewBag.PublishedCount = userPosts.Count(p => p.PostStatus?.ToLower() == "published");
            ViewBag.TotalCount = userPosts.Count();

            return View(userPosts);
        }
    }
}
