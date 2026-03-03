using JobFinder.Core.Entities.Jobs;

namespace JobFinder.UseCases.Contracts;

public interface IJobPostingRepository
{
    Task<IEnumerable<JobPosting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<JobPosting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobPosting> CreateAsync(JobPosting jobPosting, CancellationToken cancellationToken = default);
    Task UpdateAsync(JobPosting jobPosting, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}