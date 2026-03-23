using JobFinder.Core.Entities.Jobs;

namespace JobFinder.UseCases.Contracts;

public interface IJobFeedRepository
{
    Task<IEnumerable<JobPosting>> GetFeedAsync(
        string? search,
        string? category,
        string? jobType,
        string? employmentType,
        string? location,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetCandidateSkillNamesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<int> GetPublishedJobsCountAsync(CancellationToken cancellationToken = default);
}