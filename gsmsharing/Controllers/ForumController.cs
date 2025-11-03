using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using gsmsharing.Interfaces;
using gsmsharing.Models;

namespace gsmsharing.Controllers
{
    public class ForumController : Controller
    {
        private readonly IForumThreadRepository _threadRepository;
        private readonly IForumReplyRepository _replyRepository;
        private readonly ILogger<ForumController> _logger;
        private readonly IUserService _userService;

        public ForumController(
            IForumThreadRepository threadRepository,
            IForumReplyRepository replyRepository,
            ILogger<ForumController> logger,
            IUserService userService)
        {
            _threadRepository = threadRepository;
            _replyRepository = replyRepository;
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var threads = await _threadRepository.GetPublishedThreadsAsync();
                return View(threads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forum threads");
                return View(new List<ForumThread>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var thread = await _threadRepository.GetThreadByIdAsync(id);
                if (thread == null)
                {
                    return NotFound();
                }

                // Increment view count
                await _threadRepository.IncrementViewCountAsync(id);

                return View(thread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forum thread {ThreadId}", id);
                return NotFound();
            }
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumThread thread)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    thread.UserId = _userService.GetCurrentUserId();
                    thread.CreationDate = DateTime.Now;
                    thread.Views = 0;
                    thread.Likes = 0;
                    thread.Dislikes = 0;
                    thread.Publish = 1;

                    await _threadRepository.AddThreadAsync(thread);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating forum thread");
                    ModelState.AddModelError("", "Error creating thread. Please try again.");
                }
            }
            return View(thread);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var thread = await _threadRepository.GetThreadByIdAsync(id);
            var currentUserId = _userService.GetCurrentUserId();
            if (thread == null || thread.UserId != currentUserId)
            {
                return Unauthorized();
            }

            return View(thread);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ForumThread thread)
        {
            if (id != thread.UserFourmID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingThread = await _threadRepository.GetThreadByIdAsync(id);
                    var currentUserId = _userService.GetCurrentUserId();
                    if (existingThread == null || existingThread.UserId != currentUserId)
                    {
                        return Unauthorized();
                    }

                    existingThread.Title = thread.Title;
                    existingThread.Content = thread.Content;
                    existingThread.Tags = thread.Tags;
                    existingThread.MetaDiscription = thread.MetaDiscription;

                    await _threadRepository.UpdateThreadAsync(existingThread);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating forum thread {ThreadId}", id);
                    ModelState.AddModelError("", "Error updating thread. Please try again.");
                }
            }
            return View(thread);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var thread = await _threadRepository.GetThreadByIdAsync(id);
                var currentUserId = _userService.GetCurrentUserId();
                if (thread == null || thread.UserId != currentUserId)
                {
                    return Unauthorized();
                }

                await _threadRepository.DeleteThreadAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting forum thread {ThreadId}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int threadId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Reply content cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = threadId });
            }

            try
            {
                var reply = new ForumReply
                {
                    ThreadId = threadId,
                    UserId = _userService.GetCurrentUserId(),
                    ForumContent = content,
                    PublishDate = DateTime.Now,
                    Like = 0,
                    DisLike = 0,
                    Views = 0
                };

                await _replyRepository.AddReplyAsync(reply);
                TempData["Success"] = "Reply posted successfully.";
                return RedirectToAction(nameof(Details), new { id = threadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting reply to thread {ThreadId}", threadId);
                TempData["Error"] = "Error posting reply. Please try again.";
                return RedirectToAction(nameof(Details), new { id = threadId });
            }
        }
    }
}

