using JobFinder.Core.Entities.Candidates;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class CandidateRepository : ICandidateRepository
{
    private readonly ApplicationDbContext _context;

    public CandidateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateProfiles
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Skills).ThenInclude(s => s.Skill)
            .Include(c => c.Certifications)
            .Include(c => c.Languages).ThenInclude(l => l.Language)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CandidateProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateProfiles
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Skills).ThenInclude(s => s.Skill)
            .Include(c => c.Certifications)
            .Include(c => c.Languages).ThenInclude(l => l.Language)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<CandidateProfile> CreateAsync(CandidateProfile profile, CancellationToken cancellationToken = default)
    {
        _context.CandidateProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task UpdateAsync(CandidateProfile profile, CancellationToken cancellationToken = default)
    {
        _context.CandidateProfiles.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
    }
}