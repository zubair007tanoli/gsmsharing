using Fluxdox.Models;
using Fluxdox.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Fluxdox.Controllers
{
    // Designates this as an API controller, enabling automatic model validation and HTTP 400 responses.
    [ApiController]
    [Route("api")] // Base route for all actions in this controller
    public class ApiController : ControllerBase
    {
        private readonly IStorageService _storage;
        private readonly IJobQueue _jobs;
        private readonly ILogger<ApiController> _logger; // Added logging

        public ApiController(IStorageService storage, IJobQueue jobs, ILogger<ApiController> logger)
        {
            _storage = storage;
            _jobs = jobs;
            _logger = logger;
        }

        /// <summary>
        /// Generates a presigned URL for direct client-to-storage upload.
        /// Prevents server memory exhaustion during large file uploads.
        /// </summary>
        /// <param name="req">Request containing the file name.</param>
        /// <returns>A JSON object with the generated file key and presigned URL.</returns>
        [HttpPost("presign")] // Route: /api/presign
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Presign([FromBody] PresignRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FileName))
            {
                return BadRequest("File name is required.");
            }

            _logger.LogInformation("Presign request received for file: {FileName}", req.FileName);

            // Generate a unique key for the file in storage
            // In a real scenario, consider user ID in the key for logical isolation (FR-09, NFR-Security)
            var key = $"uploads/{Guid.NewGuid()}/{req.FileName}";
            var url = await _storage.GetPresignedUploadUrlAsync(key);

            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Failed to generate presigned URL for key: {Key}", key);
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate upload URL.");
            }

            return Ok(new { key, url });
        }

        /// <summary>
        /// Enqueues a PDF merge job.
        /// </summary>
        /// <param name="req">Request containing a list of file keys to merge.</param>
        /// <returns>An Accepted (202) response with the job ID.</returns>
        [HttpPost("merge")] // Route: /api/merge
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Merge([FromBody] MergeRequest req)
        {
            if (req.FileKeys == null || !req.FileKeys.Any())
            {
                return BadRequest("At least one file key is required for merging.");
            }

            _logger.LogInformation("Merge job request received with {FileCount} files.", req.FileKeys.Count);

            var job = new JobModel
            {
                Id = Guid.NewGuid().ToString("N"),
                // In a real app, get UserId from HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                UserId = User.Identity?.IsAuthenticated == true ? User.Identity.Name! : "guest",
                Status = JobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                FileKeys = req.FileKeys
            };

            await _jobs.EnqueueAsync(job);
            _logger.LogInformation("Merge job {JobId} enqueued for user {UserId}.", job.Id, job.UserId);
            return Accepted(new { jobId = job.Id }); // Returns 202 Accepted as per PRD Async Processing (5.2)
        }

        /// <summary>
        /// Retrieves the current status of a document processing job.
        /// </summary>
        /// <param name="id">The job ID.</param>
        /// <returns>The status of the job or 404 Not Found if the job does not exist.</returns>
        [HttpGet("jobs/{id}")] // Route: /api/jobs/{id}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JobStatus(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Job ID is required.");
            }

            var job = await _jobs.GetAsync(id);
            if (job == null)
            {
                _logger.LogWarning("Job with ID {JobId} not found.", id);
                return NotFound($"Job with ID '{id}' not found.");
            }
            return Ok(job);
        }
    }
}