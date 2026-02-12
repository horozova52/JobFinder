namespace JobFinder.Core.Entities.Candidates;

public class Certification
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }
    public string Name { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
