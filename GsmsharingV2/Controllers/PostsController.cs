using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommunityRepository _communityRepository;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            IPostRepository postRepository,
            ICommunityRepository communityRepository,
            ILogger<PostsController> logger)
        {
            _postRepository = postRepository;
            _communityRepository = communityRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var posts = await _postRepository.GetPaginatedAsync(page, pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = await _postRepository.GetTotalCountAsync();
            return View(posts);
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
                return NotFound();
            }

            await _postRepository.IncrementViewCountAsync(post.PostID);
            return View(post);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var communities = await _communityRepository.GetAllAsync();
            ViewBag.Communities = communities;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (!ModelState.IsValid)
            {
                var communities = await _communityRepository.GetAllAsync();
                ViewBag.Communities = communities;
                return View(post);
            }

            post.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            post.PostStatus = "published";
            post.CreatedAt = DateTime.UtcNow;
            post.PublishedAt = DateTime.UtcNow;

            await _postRepository.CreateAsync(post);
            
            var community = await _communityRepository.GetByIdAsync(post.CommunityID ?? 0);
            return RedirectToAction("Details", new { community = community?.Slug ?? "gsmsharing", slug = post.Slug });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            var communities = await _communityRepository.GetAllAsync();
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
                var communities = await _communityRepository.GetAllAsync();
                ViewBag.Communities = communities;
                return View(post);
            }

            var existingPost = await _postRepository.GetByIdAsync(post.PostID);
            if (existingPost == null)
            {
                return NotFound();
            }

                if (existingPost.UserId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            existingPost.FeaturedImage = post.FeaturedImage;
            existingPost.PostStatus = post.PostStatus;
            existingPost.CommunityID = post.CommunityID;
            existingPost.UpdatedAt = DateTime.UtcNow;

            await _postRepository.UpdateAsync(existingPost);
            
            var community = await _communityRepository.GetByIdAsync(existingPost.CommunityID ?? 0);
            return RedirectToAction("Details", new { community = community?.Slug ?? "gsmsharing", slug = existingPost.Slug });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _postRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}

