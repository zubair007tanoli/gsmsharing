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
            var communityDtos = await _communityService.GetAllAsync();
            var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
            ViewBag.Communities = communities;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                var communityDtos = await _communityService.GetAllAsync();
                var communities = _mapper.Map<IEnumerable<Community>>(communityDtos);
                ViewBag.Communities = communities;
                return View(post);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var createPostDto = _mapper.Map<CreatePostDto>(post);
            
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
            return RedirectToAction("Details", new { community = communityDto?.Slug ?? "gsmsharing", slug = createdPostDto.Slug });
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

