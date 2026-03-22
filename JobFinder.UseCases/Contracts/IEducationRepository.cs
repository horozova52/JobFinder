using JobFinder.Core.Entities.Candidates;

namespace JobFinder.UseCases.Contracts;

public interface IEducationRepository
{
    Task<Education?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Education>> GetByCandidateProfileIdAsync(int candidateProfileId, CancellationToken cancellationToken = default);
    Task<Education> CreateAsync(Education education, CancellationToken cancellationToken = default);
    Task UpdateAsync(Education education, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default);
}