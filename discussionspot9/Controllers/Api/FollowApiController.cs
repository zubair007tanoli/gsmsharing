using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace discussionspot9.Controllers.API
{
    [ApiController]
    [Route("api/follow")]
    [Authorize]
    public class FollowApiController : ControllerBase
    {
        private readonly IFollowService _followService;
        private readonly ILogger<FollowApiController> _logger;

        public FollowApiController(
            IFollowService followService,
            ILogger<FollowApiController> logger)
        {
            _followService = followService;
            _logger = logger;
        }

        /// <summary>
        /// Toggle follow/unfollow a user
        /// </summary>
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleFollow([FromBody] FollowRequest request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                if (currentUserId == request.UserId)
                {
                    return BadRequest(new { success = false, message = "You cannot follow yourself" });
                }

                var isFollowing = await _followService.IsFollowingAsync(currentUserId, request.UserId);

                bool result;
                string message;

                if (isFollowing)
                {
                    result = await _followService.UnfollowUserAsync(currentUserId, request.UserId);
                    message = result ? "Unfollowed successfully" : "Failed to unfollow";
                }
                else
                {
                    result = await _followService.FollowUserAsync(currentUserId, request.UserId);
                    message = result ? "Now following" : "Failed to follow";
                }

                return Ok(new
                {
                    success = result,
                    isFollowing = !isFollowing && result,
                    message = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling follow for user {UserId}", request.UserId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if current user follows a specific user
        /// </summary>
        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetFollowStatus(string userId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Ok(new { isFollowing = false });
                }

                var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);
                var followerCount = await _followService.GetFollowerCountAsync(userId);
                var followingCount = await _followService.GetFollowingCountAsync(userId);

                return Ok(new
                {
                    isFollowing = isFollowing,
                    followerCount = followerCount,
                    followingCount = followingCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follow status for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get suggested users to follow
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(int count = 5)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var suggestions = await _followService.GetSuggestedUsersAsync(currentUserId, count);

                return Ok(new { suggestions = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follow suggestions");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get follower count for a user
        /// </summary>
        [HttpGet("count/followers/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowerCount(string userId)
        {
            try
            {
                var count = await _followService.GetFollowerCountAsync(userId);
                return Ok(new { count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follower count for user {UserId}", userId);
                return StatusCode(500, new { count = 0 });
            }
        }

        /// <summary>
        /// Get following count for a user
        /// </summary>
        [HttpGet("count/following/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowingCount(string userId)
        {
            try
            {
                var count = await _followService.GetFollowingCountAsync(userId);
                return Ok(new { count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following count for user {UserId}", userId);
                return StatusCode(500, new { count = 0 });
            }
        }
    }

    public class FollowRequest
    {
        public string UserId { get; set; } = null!;
    }
}
