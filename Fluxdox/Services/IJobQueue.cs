using Fluxdox.Models;
using System.Threading.Tasks;

namespace Fluxdox.Services
{
    /// <summary>
    /// Defines the contract for enqueuing and retrieving background processing jobs.
    /// </summary>
    public interface IJobQueue
    {
        /// <summary>
        /// Enqueues a new job for background processing.
        /// </summary>
        /// <param name="job">The job model to enqueue.</param>
        Task EnqueueAsync(JobModel job);

        /// <summary>
        /// Retrieves a job by its ID.
        /// </summary>
        /// <param name="jobId">The ID of the job.</param>
        /// <returns>The JobModel if found, otherwise null.</returns>
        Task<JobModel?> GetAsync(string jobId);

        /// <summary>
        /// Updates the status or progress of an existing job.
        /// </summary>
        /// <param name="job">The job model with updated information.</param>
        Task UpdateAsync(JobModel job);
    }
}