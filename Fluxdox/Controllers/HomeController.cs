using Fluxdox.Models;
using Fluxdox.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fluxdox.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStorageService _storage;
        private readonly IJobQueue _jobs;

        public HomeController(IStorageService storage, IJobQueue jobs)
        {
            _storage = storage;
            _jobs = jobs;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [Route("api/presign")]
        public async Task<IActionResult> Presign([FromBody] PresignRequest req)
        {
            var key = $"uploads/{Guid.NewGuid()}/{req.FileName}";
            var url = await _storage.GetPresignedUploadUrlAsync(key);
            return Ok(new { key, url });
        }

        [HttpPost]
        [Route("api/merge")]
        public async Task<IActionResult> Merge([FromBody] MergeRequest req)
        {
            var job = new JobModel
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = "anonymous",
                Status = Fluxdox.Models.JobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                FileKeys = req.FileKeys
            };

            await _jobs.EnqueueAsync(job);
            return Accepted(new { jobId = job.Id });
        }

        [HttpGet]
        [Route("api/jobs/{id}")]
        public async Task<IActionResult> JobStatus(string id)
        {
            var job = await _jobs.GetAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class PresignRequest { public string FileName { get; set; } }
    public class MergeRequest { public List<string> FileKeys { get; set; } = new(); }
}
