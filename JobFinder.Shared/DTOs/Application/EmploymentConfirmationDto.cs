using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Application;

public class EmploymentConfirmationDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string JobTitle { get; set; } = "";
    public string CandidateFullName { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public DateTime ConfirmedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public EmploymentConfirmationStatus Status { get; set; }
}