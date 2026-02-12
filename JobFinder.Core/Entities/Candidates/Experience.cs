namespace JobFinder.Core.Entities.Candidates;

public class Experience
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Position { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
