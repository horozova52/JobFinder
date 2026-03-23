using JobFinder.Core.Entities.Jobs;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class JobFeedRepository : IJobFeedRepository
{
    private readonly ApplicationDbContext _db;

    public JobFeedRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<JobPosting>> GetFeedAsync(
        string? search,
        string? category,
        string? jobType,
        string? employmentType,
        string? location,
        CancellationToken cancellationToken = default)
    {
        var query = _db.JobPostings
            .Include(j => j.EmployerProfile)
            .Include(j => j.Skills)
                .ThenInclude(s => s.Skill)
            .Where(j => j.Status == JobStatus.Published)
            .AsNoTracking();

        // Filtru search — titlu sau companie
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(j =>
                j.Title.Contains(search) ||
                j.EmployerProfile.CompanyName.Contains(search) ||
                (j.Location != null && j.Location.Contains(search)));

        // Filtru tip job (Remote / La birou / Hibrid)
        if (!string.IsNullOrWhiteSpace(jobType) &&
            Enum.TryParse<JobType>(jobType, ignoreCase: true, out var jt))
            query = query.Where(j => j.JobType == jt);

        // Filtru tip angajare (Full-time, Part-time etc.)
        if (!string.IsNullOrWhiteSpace(employmentType) &&
            Enum.TryParse<EmploymentType>(employmentType, ignoreCase: true, out var et))
            query = query.Where(j => j.EmploymentType == et);

        // Filtru locație (text simplu)
        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(j =>
                j.Location != null && j.Location.Contains(location));

        // Filtru categorie — căutăm după skill-uri care au categoria respectivă
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(j =>
                j.Skills.Any(s =>
                    s.Skill != null &&
                    s.Skill.Category != null &&
                    s.Skill.Category.Contains(category)));

        return await query
            .OrderByDescending(j => j.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetCandidateSkillNamesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.CandidateSkills
            .Include(cs => cs.Skill)
            .Where(cs => cs.CandidateProfile.UserId == userId)
            .Select(cs => cs.Skill.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetPublishedJobsCountAsync(
    CancellationToken cancellationToken = default)
    {
        return await _db.JobPostings
            .CountAsync(j => j.Status == JobStatus.Published, cancellationToken);
    }
}