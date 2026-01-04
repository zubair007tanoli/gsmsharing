using Fluxdox.Models;
using System.Threading.Tasks;

namespace Fluxdox.Services
{
    public interface IJobQueue
    {
        Task EnqueueAsync(JobModel job);
        Task<JobModel> GetAsync(string jobId);
    }
}