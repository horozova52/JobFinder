using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Common;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class CandidateSkillRepository : ICandidateSkillRepository
{
    private readonly ApplicationDbContext _context;

    public CandidateSkillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateSkill?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateSkills
            .Include(s => s.Skill)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<CandidateSkill>> GetByCandidateProfileIdAsync(int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateSkills
            .Include(s => s.Skill)
            .AsNoTracking()
            .Where(s => s.CandidateProfileId == candidateProfileId)
            .OrderBy(s => s.Skill.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CandidateSkill> CreateAsync(CandidateSkill skill, CancellationToken cancellationToken = default)
    {
        _context.CandidateSkills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-fetch cu Skill inclus pentru AutoMapper (SkillName)
        return await _context.CandidateSkills
            .Include(s => s.Skill)
            .FirstAsync(s => s.Id == skill.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var skill = await _context.CandidateSkills.FindAsync([id], cancellationToken);
        if (skill != null)
        {
            _context.CandidateSkills.Remove(skill);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateSkills
            .AnyAsync(s => s.Id == id && s.CandidateProfileId == candidateProfileId, cancellationToken);
    }

    public async Task<bool> AlreadyHasSkillAsync(int candidateProfileId, int skillId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateSkills
            .AnyAsync(s => s.CandidateProfileId == candidateProfileId && s.SkillId == skillId, cancellationToken);
    }

    public async Task<Skill> FindOrCreateSkillAsync(string skillName, CancellationToken cancellationToken = default)
    {
        var normalized = skillName.Trim();

        var existing = await _context.Skills
            .FirstOrDefaultAsync(
                s => s.Name.ToLower() == normalized.ToLower(),
                cancellationToken);

        if (existing != null)
            return existing;

        var newSkill = new Skill
        {
            Name = normalized,
            Category = null  // skill personalizat — fără categorie predefinită
        };

        _context.Skills.Add(newSkill);
        await _context.SaveChangesAsync(cancellationToken);

        // newSkill.Id e populat de EF după SaveChanges
        return newSkill;
    }
}