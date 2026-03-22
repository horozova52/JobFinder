using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Candidates;

public class UpdateCandidateProfileDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Headline { get; set; }
    public string? Location { get; set; }
    public string? Summary { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LinkedIn { get; set; }
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public CandidateStatus Status { get; set; } = CandidateStatus.ActivelyLooking;
    public int? PreferredJobCategoryId { get; set; }
    public JobType? PreferredJobType { get; set; }
}