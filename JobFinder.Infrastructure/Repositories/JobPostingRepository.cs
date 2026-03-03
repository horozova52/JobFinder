using JobFinder.Core.Entities.Jobs;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class JobPostingRepository : IJobPostingRepository
{
    private readonly ApplicationDbContext _context;

    public JobPostingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobPosting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .Include(j => j.Skills)
                .ThenInclude(s => s.Skill)
            .Include(j => j.EmployerProfile)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<JobPosting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .Include(j => j.Skills)
                .ThenInclude(s => s.Skill)
            .Include(j => j.EmployerProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<JobPosting> CreateAsync(JobPosting jobPosting, CancellationToken cancellationToken = default)
    {
        jobPosting.CreatedAt = DateTime.UtcNow;
        _context.JobPostings.Add(jobPosting);
        await _context.SaveChangesAsync(cancellationToken);
        return jobPosting;
    }

    public async Task UpdateAsync(JobPosting jobPosting, CancellationToken cancellationToken = default)
    {
        _context.JobPostings.Update(jobPosting);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var jobPosting = await _context.JobPostings.FindAsync([id], cancellationToken);
        if (jobPosting is not null)
        {
            _context.JobPostings.Remove(jobPosting);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .AnyAsync(j => j.Id == id, cancellationToken);
    }
}