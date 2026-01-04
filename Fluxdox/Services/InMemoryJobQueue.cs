using Fluxdox.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Fluxdox.Services
{
    public class InMemoryJobQueue : IJobQueue
    {
        private readonly ConcurrentDictionary<string, JobModel> _jobs = new();

        public Task EnqueueAsync(JobModel job)
        {
            _jobs[job.Id] = job;
            return Task.CompletedTask;
        }

        public Task<JobModel> GetAsync(string jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return Task.FromResult(job);
        }
    }
}