using JobFinder.Core.Entities.Applications;

namespace JobFinder.UseCases.Contracts;

public interface IApplicationRepository
{
    Task<IEnumerable<Application>> GetByJobPostingIdAsync(int jobPostingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Application>> GetByCandidateIdAsync(int candidateProfileId, CancellationToken cancellationToken = default);
    Task<Application?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Application> CreateAsync(Application application, CancellationToken cancellationToken = default);
    Task UpdateAsync(Application application, CancellationToken cancellationToken = default);
    Task<bool> AlreadyAppliedAsync(int jobPostingId, int candidateProfileId, CancellationToken cancellationToken = default);
}