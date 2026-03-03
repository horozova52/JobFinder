using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Applications;

public class ApplicationStatusHistoryDto
{
    public ApplicationState Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedByUserId { get; set; }
}
