using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.Controllers
{
    [Authorize]
    public class ReactionsController : Controller
    {
        private readonly IReactionService _reactionService;
        private readonly ILogger<ReactionsController> _logger;

        public ReactionsController(IReactionService reactionService, ILogger<ReactionsController> logger)
        {
            _reactionService = reactionService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSummary(int? postId, int? commentId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var summary = await _reactionService.GetReactionSummaryAsync(postId, commentId, userId);
            return Json(summary);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] CreateReactionDto createReactionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var reaction = await _reactionService.ToggleReactionAsync(createReactionDto, userId);
                return Json(new { success = true, reaction });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling reaction");
                return StatusCode(500, new { success = false, message = "Error toggling reaction" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _reactionService.DeleteReactionAsync(id, userId);
            if (!success)
                return Forbid();

            return Json(new { success = true });
        }
    }
}

