using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Common;

namespace JobFinder.UseCases.Contracts;

public interface ICandidateSkillRepository
{
    Task<CandidateSkill?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<CandidateSkill>> GetByCandidateProfileIdAsync(int candidateProfileId, CancellationToken cancellationToken = default);
    Task<CandidateSkill> CreateAsync(CandidateSkill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default);
    Task<bool> AlreadyHasSkillAsync(int candidateProfileId, int skillId, CancellationToken cancellationToken = default);

    Task<Skill> FindOrCreateSkillAsync(string skillName, CancellationToken cancellationToken = default);
}