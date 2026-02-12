using JobFinder.Shared.Enums;

namespace JobFinder.Core.Entities.Applications;

public class ApplicationStatusHistory
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public ApplicationState Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedByUserId { set; get; }

    public Application Application { get; set; } = null!;
}
