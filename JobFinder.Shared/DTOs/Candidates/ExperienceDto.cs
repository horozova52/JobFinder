using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Candidates;

public class ExperienceDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Position { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int? EmployerProfileId { get; set; }
    public ExperienceStatus Status { get; set; } = ExperienceStatus.Manual;
    public EmploymentType? EmploymentType { get; set; }
}