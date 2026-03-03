using JobFinder.Core.Entities.Candidates;

namespace JobFinder.UseCases.Contracts;

public interface ICandidateRepository
{
    Task<CandidateProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CandidateProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CandidateProfile> CreateAsync(CandidateProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(CandidateProfile profile, CancellationToken cancellationToken = default);
}