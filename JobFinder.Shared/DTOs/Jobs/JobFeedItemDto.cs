namespace JobFinder.Shared.DTOs.Jobs;

public class JobFeedItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string? CompanyLogoUrl { get; set; }
    public int? EmployerProfileId { get; set; }
    public string? Location { get; set; }
    public string JobType { get; set; } = "";
    public string EmploymentType { get; set; } = "";
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public bool IsSalaryNegotiable { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<string> SkillNames { get; set; } = new();


    public int MatchScore { get; set; }     
    public int MatchedSkillsCount { get; set; }
    public int TotalSkillsRequired { get; set; }
}