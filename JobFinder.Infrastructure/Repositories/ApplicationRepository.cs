using JobFinder.Core.Entities.Applications;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Application>> GetByJobPostingIdAsync(int jobPostingId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.CandidateProfile)
            .Include(a => a.StatusHistory)
            .Where(a => a.JobPostingId == jobPostingId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Application>> GetByCandidateIdAsync(int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.JobPosting)
                .ThenInclude(j => j.EmployerProfile)
            .Include(a => a.StatusHistory)
            .Where(a => a.CandidateProfileId == candidateProfileId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Application?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .Include(a => a.JobPosting)
            .Include(a => a.CandidateProfile)
            .Include(a => a.StatusHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Application> CreateAsync(Application application, CancellationToken cancellationToken = default)
    {
        application.AppliedAt = DateTime.UtcNow;
        application.Status = Shared.Enums.ApplicationState.Pending;
        _context.Applications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);
        return application;
    }

    public async Task UpdateAsync(Application application, CancellationToken cancellationToken = default)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AlreadyAppliedAsync(int jobPostingId, int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.JobPostingId == jobPostingId &&
                          a.CandidateProfileId == candidateProfileId,
                      cancellationToken);
    }
}