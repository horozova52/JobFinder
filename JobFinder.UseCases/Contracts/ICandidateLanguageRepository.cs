using JobFinder.Core.Entities.Candidates;

namespace JobFinder.UseCases.Contracts;

public interface ICandidateLanguageRepository
{
    Task<CandidateLanguage?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CandidateLanguage> CreateAsync(CandidateLanguage language, CancellationToken cancellationToken = default);
    Task UpdateAsync(CandidateLanguage language, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default);
    Task<bool> AlreadyHasLanguageAsync(int candidateProfileId, int languageId, CancellationToken cancellationToken = default);
}