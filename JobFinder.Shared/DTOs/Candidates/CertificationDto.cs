namespace JobFinder.Shared.DTOs.Candidates;

public class CertificationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
