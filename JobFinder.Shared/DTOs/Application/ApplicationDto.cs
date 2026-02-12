using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Applications;

public class ApplicationDto
{
    public int Id { get; set; }

    public int JobPostingId { get; set; }
    public string JobTitle { get; set; } = null!;

    public int CandidateProfileId { get; set; }
    public string CandidateFullName { get; set; } = null!;

    public DateTime AppliedAt { get; set; }
    public ApplicationState Status { get; set; }

    public string? CoverLetter { get; set; }
}
