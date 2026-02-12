using JobFinder.Shared.Enums;
using JobFinder.Core.Entities.Employers;

namespace JobFinder.Core.Entities.Validation;

public class EmployerVerification
{
    public int Id { get; set; }

    public int EmployerProfileId { get; set; }

    public EmployerVerificationStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
    public string? Comment { get; set; }

    public EmployerProfile EmployerProfile { get; set; } = null!;
}
