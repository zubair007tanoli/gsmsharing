using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollDiagnosticController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PollDiagnosticController> _logger;

        public PollDiagnosticController(ApplicationDbContext context, ILogger<PollDiagnosticController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("check/{postId}")]
        public async Task<IActionResult> CheckPoll(int postId)
        {
            try
            {
                var result = new
                {
                    PostId = postId,
                    Post = await _context.Posts
                        .Where(p => p.PostId == postId)
                        .Select(p => new { p.PostId, p.Title, p.HasPoll, p.PollOptionCount })
                        .FirstOrDefaultAsync(),
                    
                    PollOptions = await _context.PollOptions
                        .Where(po => po.PostId == postId)
                        .Select(po => new { po.PollOptionId, po.OptionText, po.VoteCount, po.DisplayOrder })
                        .OrderBy(po => po.DisplayOrder)
                        .ToListAsync(),
                    
                    PollConfiguration = await _context.PollConfigurations
                        .Where(pc => pc.PostId == postId)
                        .FirstOrDefaultAsync(),
                    
                    PollVotes = await _context.PollVotes
                        .Where(pv => pv.PollOption.PostId == postId)
                        .Include(pv => pv.PollOption)
                        .Select(pv => new { pv.UserId, pv.PollOptionId, pv.VotedAt, pv.PollOption.OptionText })
                        .ToListAsync(),
                    
                    CanIncludeNavigationProperty = "Testing if Include works"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking poll {postId}");
                return Ok(new { 
                    Error = ex.Message, 
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}

