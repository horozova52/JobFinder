using JobFinder.Core.Entities.Candidates;

namespace JobFinder.UseCases.Contracts;

public interface IExperienceRepository
{
    Task<Experience?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Experience>> GetByCandidateProfileIdAsync(int candidateProfileId, CancellationToken cancellationToken = default);
    Task<Experience> CreateAsync(Experience experience, CancellationToken cancellationToken = default);
    Task UpdateAsync(Experience experience, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default);
}