using JobFinder.Shared.Enums;

namespace JobFinder.Core.Entities.Applications;

public class EmploymentConfirmation
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int CandidateProfileId { get; set; }
    public int EmployerProfileId { get; set; }

   
    public DateTime ConfirmedAt { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public EmploymentConfirmationStatus Status { get; set; }

    public bool AddToExperience { get; set; } = true;

    public Application Application { get; set; } = null!;
}