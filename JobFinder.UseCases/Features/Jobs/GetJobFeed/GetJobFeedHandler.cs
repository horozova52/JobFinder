using JobFinder.Shared.DTOs.Jobs;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Queries.GetJobFeed;

public class GetJobFeedHandler : IRequestHandler<GetJobFeedQuery, GetJobFeedResult>
{
    private readonly IJobFeedRepository _feedRepo;

    public GetJobFeedHandler(IJobFeedRepository feedRepo)
    {
        _feedRepo = feedRepo;
    }

    public async Task<GetJobFeedResult> Handle(
        GetJobFeedQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Preluăm joburile filtrate
        var jobs = await _feedRepo.GetFeedAsync(
            request.Search,
            request.Category,
            request.JobType,
            request.EmploymentType,
            request.Location,
            cancellationToken);

        // 2. Preluăm skill-urile candidatului (dacă e autentificat)
        var candidateSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(request.UserId))
        {
            var skills = await _feedRepo.GetCandidateSkillNamesAsync(
                request.UserId, cancellationToken);
            foreach (var s in skills)
                candidateSkills.Add(s);
        }

        // 3. Construim DTO-urile cu matching score
        var items = jobs.Select(job =>
        {
            var jobSkillNames = job.Skills
                .Select(s => s.Skill?.Name ?? "")
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            int matched = jobSkillNames
                .Count(sn => candidateSkills.Contains(sn));

            int matchScore = jobSkillNames.Count > 0
                ? (int)Math.Round((double)matched / jobSkillNames.Count * 100)
                : 0;

            return new JobFeedItemDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.EmployerProfile?.CompanyName ?? "—",
                CompanyLogoUrl = job.EmployerProfile?.LogoUrl,
                EmployerProfileId = job.EmployerProfileId,
                Location = job.Location,
                JobType = GetJobTypeLabel(job.JobType),
                EmploymentType = GetEmploymentTypeLabel(job.EmploymentType),
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                IsSalaryNegotiable = job.IsSalaryNegotiable,
                PublishedAt = job.PublishedAt,
                SkillNames = jobSkillNames,
                MatchScore = matchScore,
                MatchedSkillsCount = matched,
                TotalSkillsRequired = jobSkillNames.Count,
            };
        })
        .OrderByDescending(x => candidateSkills.Count > 0 ? x.MatchScore : 0)
        .ThenByDescending(x => x.PublishedAt)
        .Take(request.PageSize)
        .ToList();

        return new GetJobFeedResult(items, items.Count);
    }

    private static string GetJobTypeLabel(JobType t) => t switch
    {
        JobType.Remote => "Remote",
        JobType.OnSite => "La birou",
        JobType.Hybrid => "Hibrid",
        _ => t.ToString()
    };

    private static string GetEmploymentTypeLabel(EmploymentType t) => t switch
    {
        EmploymentType.FullTime => "Full-time",
        EmploymentType.PartTime => "Part-time",
        EmploymentType.Freelance => "Freelance",
        EmploymentType.Internship => "Internship",
        EmploymentType.Contract => "Contract",
        _ => t.ToString()
    };
}