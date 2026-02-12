using JobFinder.Shared.Enums;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Jobs;

namespace JobFinder.Core.Entities.Applications;

public class Application
{
    public int Id { get; set; }

    public int JobPostingId { get; set; }
    public int CandidateProfileId { get; set; }

    public DateTime AppliedAt { get; set; }
    public Shared.Enums.ApplicationState Status { get; set; }

    public string? CoverLetter { get; set; }
    public string? Notes { get; set; }    
    public JobPosting JobPosting { get; set; } = null!;
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
}
