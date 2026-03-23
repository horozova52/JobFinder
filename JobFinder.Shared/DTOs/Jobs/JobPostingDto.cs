using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Jobs;

public class JobPostingDto
{
    public int Id { get; set; }
    public int EmployerProfileId { get; set; }
    public string? CompanyName { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }

    public string? Location { get; set; }

    public JobType JobType { get; set; }
    public EmploymentType EmploymentType { get; set; }

    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public bool IsSalaryNegotiable { get; set; }

    public JobStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public List<JobSkillDto> Skills { get; set; } = [];
}
