using Fluxdox.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace Fluxdox.Services
{
    // Mocking Redis with an in-memory ConcurrentDictionary for simplicity.
    // In production, this would use a Redis client library (e.g., StackExchange.Redis).
    public class RedisJobQueue : IJobQueue
    {
        private readonly ConcurrentDictionary<string, JobModel> _jobs = new ConcurrentDictionary<string, JobModel>();
        private readonly ILogger<RedisJobQueue> _logger;

        public RedisJobQueue(ILogger<RedisJobQueue> logger)
        {
            _logger = logger;
        }

        public Task EnqueueAsync(JobModel job)
        {
            _jobs[job.Id] = job;
            _logger.LogInformation("Job {JobId} enqueued in mock Redis. Status: {Status}", job.Id, job.Status);
            // In a real Redis implementation: PUSH job to a Redis List/Queue
            // Optionally PUBLISH a message for SignalR clients using Redis Pub/Sub
            return Task.CompletedTask;
        }

        public Task<JobModel?> GetAsync(string jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            _logger.LogInformation("Job {JobId} retrieved from mock Redis. Found: {Found}", jobId, job != null);
            // In a real Redis implementation: GET job data (e.g., from a Redis Hash)
            return Task.FromResult(job);
        }

        public Task UpdateAsync(JobModel job)
        {
            if (_jobs.ContainsKey(job.Id))
            {
                _jobs[job.Id] = job;
                _logger.LogInformation("Job {JobId} updated in mock Redis. Status: {Status}", job.Id, job.Status);
                // In a real Redis implementation: SET job data (e.g., update a Redis Hash)
                // And PUBLISH update for SignalR
            }
            else
            {
                _logger.LogWarning("Attempted to update non-existent job {JobId} in mock Redis.", job.Id);
            }
            return Task.CompletedTask;
        }
    }
}