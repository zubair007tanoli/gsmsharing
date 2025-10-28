using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowApiController : ControllerBase
    {
        private readonly IFollowService _followService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowApiController> _logger;

        public FollowApiController(
            IFollowService followService,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context,
            ILogger<FollowApiController> logger)
        {
            _followService = followService;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Follow a user
        /// </summary>
        [HttpPost("{userId}")]
        public async Task<IActionResult> FollowUser(string userId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (currentUserId == userId)
                {
                    return BadRequest(new { message = "You cannot follow yourself" });
                }

                // Check if user exists
                var userExists = await _userManager.FindByIdAsync(userId);
                if (userExists == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var success = await _followService.FollowUserAsync(currentUserId, userId);

                if (success)
                {
                    var followerCount = await _followService.GetFollowerCountAsync(userId);
                    return Ok(new
                    {
                        message = "Successfully followed user",
                        isFollowing = true,
                        followerCount = followerCount
                    });
                }

                return BadRequest(new { message = "Failed to follow user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FollowUser API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Unfollow a user
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> UnfollowUser(string userId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var success = await _followService.UnfollowUserAsync(currentUserId, userId);

                if (success)
                {
                    var followerCount = await _followService.GetFollowerCountAsync(userId);
                    return Ok(new
                    {
                        message = "Successfully unfollowed user",
                        isFollowing = false,
                        followerCount = followerCount
                    });
                }

                return BadRequest(new { message = "Failed to unfollow user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UnfollowUser API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Toggle follow status
        /// </summary>
        [HttpPost("toggle/{userId}")]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (currentUserId == userId)
                {
                    return BadRequest(new { message = "You cannot follow yourself" });
                }

                var success = await _followService.ToggleFollowAsync(currentUserId, userId);
                var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);
                var followerCount = await _followService.GetFollowerCountAsync(userId);

                return Ok(new
                {
                    message = isFollowing ? "Successfully followed user" : "Successfully unfollowed user",
                    isFollowing = isFollowing,
                    followerCount = followerCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleFollow API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get followers for a user
        /// </summary>
        [HttpGet("followers/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowers(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var followers = await _followService.GetFollowersAsync(userId, page, pageSize);
                var totalCount = await _followService.GetFollowerCountAsync(userId);

                var followersList = await Task.WhenAll(followers.Select(async f =>
                {
                    var profile = await _context.UserProfiles
                        .FirstOrDefaultAsync(up => up.UserId == f.FollowerId);

                    var currentUserId = _userManager.GetUserId(User);
                    var isFollowing = !string.IsNullOrEmpty(currentUserId) &&
                                    await _followService.IsFollowingAsync(currentUserId, f.FollowerId);

                    return new
                    {
                        userId = f.FollowerId,
                        displayName = profile?.DisplayName ?? f.Follower?.UserName ?? "Unknown",
                        avatarUrl = profile?.AvatarUrl,
                        bio = profile?.Bio,
                        karmaPoints = profile?.KarmaPoints ?? 0,
                        followedAt = f.FollowedAt,
                        isFollowing = isFollowing
                    };
                }));

                return Ok(new
                {
                    followers = followersList,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFollowers API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get users that a user is following
        /// </summary>
        [HttpGet("following/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFollowing(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var following = await _followService.GetFollowingAsync(userId, page, pageSize);
                var totalCount = await _followService.GetFollowingCountAsync(userId);

                var followingList = await Task.WhenAll(following.Select(async f =>
                {
                    var profile = await _context.UserProfiles
                        .FirstOrDefaultAsync(up => up.UserId == f.FollowedId);

                    var currentUserId = _userManager.GetUserId(User);
                    var isFollowing = !string.IsNullOrEmpty(currentUserId) &&
                                    await _followService.IsFollowingAsync(currentUserId, f.FollowedId);

                    return new
                    {
                        userId = f.FollowedId,
                        displayName = profile?.DisplayName ?? f.Followed?.UserName ?? "Unknown",
                        avatarUrl = profile?.AvatarUrl,
                        bio = profile?.Bio,
                        karmaPoints = profile?.KarmaPoints ?? 0,
                        followedAt = f.FollowedAt,
                        isFollowing = isFollowing
                    };
                }));

                return Ok(new
                {
                    following = followingList,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFollowing API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get suggested users to follow
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] int count = 5)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var suggestedUserIds = await _followService.GetSuggestedUsersAsync(currentUserId, count);

                var suggestions = await Task.WhenAll(suggestedUserIds.Select(async userId =>
                {
                    var profile = await _context.UserProfiles
                        .FirstOrDefaultAsync(up => up.UserId == userId);

                    var user = await _userManager.FindByIdAsync(userId);
                    var followerCount = await _followService.GetFollowerCountAsync(userId);
                    var mutualFollowers = await _followService.GetMutualFollowersAsync(currentUserId, userId);

                    return new
                    {
                        userId = userId,
                        displayName = profile?.DisplayName ?? user?.UserName ?? "Unknown",
                        avatarUrl = profile?.AvatarUrl,
                        bio = profile?.Bio,
                        karmaPoints = profile?.KarmaPoints ?? 0,
                        followerCount = followerCount,
                        mutualFollowerCount = mutualFollowers.Count
                    };
                }));

                return Ok(new { suggestions = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSuggestions API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if current user follows another user
        /// </summary>
        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetFollowStatus(string userId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
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
                _logger.LogError(ex, "Error in GetFollowStatus API");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}

