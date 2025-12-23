using AutoMapper;
using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using GsmsharingV2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommunityService _communityService;
        private readonly IMapper _mapper;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IPostService postService,
            ICommunityService communityService,
            IMapper mapper,
            ILogger<PostsController> logger)
        {
            _postService = postService;
            _communityService = communityService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var postDtos = await _postService.GetPaginatedAsync(page, pageSize);
            var posts = _mapper.Map<IEnumerable<Post>>(postDtos);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = await _postService.GetTotalCountAsync();
            return View(posts);
        }

        [HttpGet]
        [Route("/r/{community}/{slug}", Name = "PostDetails")]
        public async Task<IActionResult> Details(string community, string slug)
        {
            if (string.IsNullOrEmpty(community) || string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var postDto = await _postService.GetBySlugAndCommunityAsync(slug, community);
            
            if (postDto == null)
            {
                return NotFound();
            }

            await _postService.IncrementViewCountAsync(postDto.PostID);
            var post = _mapper.Map<Post>(postDto);
            return View(post);
        }

        [Authorize]
        [HttpGet]

        public async Task<IActionResult> Create()
        {
            try
            {
                var communityDtos = await _communityService.GetAllAsync();
                var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
                ViewBag.Communities = communities ?? Enumerable.Empty<Community>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading communities for create post");
                ViewBag.Communities = Enumerable.Empty<Community>();
            }
            
            // Initialize model with default values to avoid null reference issues
            var model = new Post
            {
                AllowComments = true, // Default to true
                PostStatus = "published"
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Post post, IFormFile? imageFile, string action)
        {
            // Manually bind all properties to avoid nullable boolean binding issues
            var createPostDto = new CreatePostDto
            {
                Title = Request.Form["Title"].ToString(),
                Slug = Request.Form["Slug"].ToString(),
                Content = Request.Form["Content"].ToString(),
                Excerpt = Request.Form["Excerpt"].ToString(),
                Tags = Request.Form["Tags"].ToString(),
                FeaturedImage = Request.Form["FeaturedImage"].ToString(),
                MetaTitle = Request.Form["MetaTitle"].ToString(),
                MetaDescription = Request.Form["MetaDescription"].ToString(),
                OgTitle = Request.Form["OgTitle"].ToString(),
                OgDescription = Request.Form["OgDescription"].ToString(),
                OgImage = Request.Form["OgImage"].ToString(),
                CanonicalUrl = Request.Form["CanonicalUrl"].ToString(),
                FocusKeyword = Request.Form["FocusKeyword"].ToString(),
                CommunityID = int.TryParse(Request.Form["CommunityID"].ToString(), out var communityId) ? communityId : null
            };
            
            // Handle post status based on action button
            if (action == "draft")
            {
                createPostDto.PostStatus = "draft";
            }
            else
            {
                createPostDto.PostStatus = "published";
            }
            
            // Handle nullable boolean checkboxes from form
            createPostDto.AllowComments = Request.Form.ContainsKey("AllowComments") && Request.Form["AllowComments"].ToString() == "true";
            createPostDto.IsFeatured = Request.Form.ContainsKey("IsFeatured") && Request.Form["IsFeatured"].ToString() == "true";
            createPostDto.IsPromoted = Request.Form.ContainsKey("IsPromoted") && Request.Form["IsPromoted"].ToString() == "true";
            
            // Handle non-nullable booleans from form
            createPostDto.IsPinned = Request.Form.ContainsKey("IsPinned") && Request.Form["IsPinned"].ToString() == "true";
            createPostDto.IsLocked = Request.Form.ContainsKey("IsLocked") && Request.Form["IsLocked"].ToString() == "true";
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createPostDto.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }
            if (createPostDto.CommunityID == null || createPostDto.CommunityID == 0)
            {
                ModelState.AddModelError("CommunityID", "Community is required");
            }
            if (string.IsNullOrWhiteSpace(createPostDto.Content))
            {
                ModelState.AddModelError("Content", "Content is required");
            }
            
            if (!ModelState.IsValid)
            {
                var communityDtos = await _communityService.GetAllAsync();
                var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
                ViewBag.Communities = communities;
                var model = new Post
                {
                    Title = createPostDto.Title,
                    Content = createPostDto.Content,
                    CommunityID = createPostDto.CommunityID,
                    AllowComments = createPostDto.AllowComments ?? true,
                    PostStatus = createPostDto.PostStatus
                };
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            
            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    var imageUploadService = HttpContext.RequestServices.GetRequiredService<IImageUploadService>();
                    var imagePath = await imageUploadService.UploadImageAsync(imageFile, "posts");
                    createPostDto.FeaturedImage = imagePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image");
                    ModelState.AddModelError("FeaturedImage", "Error uploading image: " + ex.Message);
                    var communityDtos = await _communityService.GetAllAsync();
                    var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
                    ViewBag.Communities = communities;
                    return View(post);
                }
            }
            
            var createdPostDto = await _postService.CreateAsync(createPostDto, userId);
            
            var communityDto = await _communityService.GetByIdAsync(createdPostDto.CommunityID ?? 0);
            
            // SEO-optimized URL: /posts/{community-slug}/{post-slug}
            return RedirectToAction("Details", new { 
                community = communityDto?.Slug ?? "gsmsharing", 
                slug = createdPostDto.Slug 
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var postDto = await _postService.GetByIdAsync(id);
            if (postDto == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (postDto.UserId != userId)
            {
                return Forbid();
            }

            var post = _mapper.Map<Post>(postDto);
            var communityDtos = await _communityService.GetAllAsync();
            var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
            ViewBag.Communities = communities;
            return View(post);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Post post)
        {
            if (!ModelState.IsValid)
            {
                var communityDtos = await _communityService.GetAllAsync();
                var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
                ViewBag.Communities = communities;
                return View(post);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var updatePostDto = _mapper.Map<UpdatePostDto>(post);
            
            try
            {
                var updatedPostDto = await _postService.UpdateAsync(updatePostDto, userId);
                var communityDto = await _communityService.GetByIdAsync(updatedPostDto.CommunityID ?? 0);
                return RedirectToAction("Details", new { community = communityDto?.Slug ?? "gsmsharing", slug = updatedPostDto.Slug });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            
            try
            {
                // Check if user is admin
                if (User.IsInRole("Admin"))
                {
                    // Admin can delete any post - we'll need to handle this differently
                    // For now, we'll check ownership in the service
                    var postDto = await _postService.GetByIdAsync(id);
                    if (postDto == null)
                    {
                        return NotFound();
                    }
                }

                await _postService.DeleteAsync(id, userId);
                return RedirectToAction("Index");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}

