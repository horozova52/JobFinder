using JobFinder.Core.Entities.Applications;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class EmploymentConfirmationRepository : IEmploymentConfirmationRepository
{
    private readonly ApplicationDbContext _db;

    public EmploymentConfirmationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<EmploymentConfirmation?> GetByApplicationIdAsync(
        int applicationId, CancellationToken ct = default)
        => await _db.EmploymentConfirmations
            .Include(e => e.Application)
            .FirstOrDefaultAsync(e => e.ApplicationId == applicationId, ct);

    public async Task<List<EmploymentConfirmation>> GetPendingByEmployerAsync(
        int employerProfileId, CancellationToken ct = default)
        => await _db.EmploymentConfirmations
            .Include(e => e.Application)
                .ThenInclude(a => a.CandidateProfile)
            .Include(e => e.Application)
                .ThenInclude(a => a.JobPosting)
            .Where(e => e.EmployerProfileId == employerProfileId &&
                        e.Status == EmploymentConfirmationStatus.PendingValidation)
            .ToListAsync(ct);

    public async Task<EmploymentConfirmation> CreateAsync(
        EmploymentConfirmation confirmation, CancellationToken ct = default)
    {
        _db.EmploymentConfirmations.Add(confirmation);
        await _db.SaveChangesAsync(ct);
        return confirmation;
    }

    public async Task UpdateAsync(
        EmploymentConfirmation confirmation, CancellationToken ct = default)
    {
        _db.EmploymentConfirmations.Update(confirmation);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsForApplicationAsync(
        int applicationId, CancellationToken ct = default)
        => await _db.EmploymentConfirmations
            .AnyAsync(e => e.ApplicationId == applicationId, ct);
}