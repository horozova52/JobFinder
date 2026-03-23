using JobFinder.Core.Entities.Employers;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class EmployerRepository : IEmployerRepository
{
    private readonly ApplicationDbContext _context;

    public EmployerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EmployerProfiles
            .Include(e => e.Locations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmployerProfile?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployerProfiles
            .Include(e => e.Locations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<EmployerProfile> CreateAsync(EmployerProfile profile, CancellationToken cancellationToken = default)
    {
        _context.EmployerProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task UpdateAsync(EmployerProfile profile, CancellationToken cancellationToken = default)
    {
        _context.EmployerProfiles.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<EmployerProfile?> GetTrackedByUserIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployerProfiles
            .Include(e => e.Locations)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }
}