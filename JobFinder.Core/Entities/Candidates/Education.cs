namespace JobFinder.Core.Entities.Candidates;

public class Education
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }
    public string Institution { get; set; } = null!;
    public string Degree { get; set; } = null!;      
    public string? FieldOfStudy { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
