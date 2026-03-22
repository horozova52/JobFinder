using JobFinder.Core.Entities.Candidates;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class ExperienceRepository : IExperienceRepository
{
    private readonly ApplicationDbContext _context;

    public ExperienceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Experience?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Experiences
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Experience>> GetByCandidateProfileIdAsync(int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Experiences
            .AsNoTracking()
            .Where(e => e.CandidateProfileId == candidateProfileId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Experience> CreateAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        _context.Experiences.Add(experience);
        await _context.SaveChangesAsync(cancellationToken);
        return experience;
    }

    public async Task UpdateAsync(Experience experience, CancellationToken cancellationToken = default)
    {
        _context.Experiences.Update(experience);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var experience = await _context.Experiences.FindAsync([id], cancellationToken);
        if (experience != null)
        {
            _context.Experiences.Remove(experience);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Experiences
            .AnyAsync(e => e.Id == id && e.CandidateProfileId == candidateProfileId, cancellationToken);
    }
}