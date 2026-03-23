using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Jobs;

public class CreateJobPostingDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public JobType JobType { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public bool IsSalaryNegotiable { get; set; }
    public string? Tags { get; set; }
}
