using JobFinder.Core.Entities.Candidates;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class EducationRepository : IEducationRepository
{
    private readonly ApplicationDbContext _context;

    public EducationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Education?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Educations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Education>> GetByCandidateProfileIdAsync(int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Educations
            .AsNoTracking()
            .Where(e => e.CandidateProfileId == candidateProfileId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Education> CreateAsync(Education education, CancellationToken cancellationToken = default)
    {
        _context.Educations.Add(education);
        await _context.SaveChangesAsync(cancellationToken);
        return education;
    }

    public async Task UpdateAsync(Education education, CancellationToken cancellationToken = default)
    {
        _context.Educations.Update(education);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var education = await _context.Educations.FindAsync([id], cancellationToken);
        if (education != null)
        {
            _context.Educations.Remove(education);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Educations
            .AnyAsync(e => e.Id == id && e.CandidateProfileId == candidateProfileId, cancellationToken);
    }
}