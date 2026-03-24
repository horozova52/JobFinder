namespace JobFinder.Shared.DTOs.Admin;

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalEmployers { get; set; }
    public int ActiveJobs { get; set; }
    public int TotalApps { get; set; }
    public int PendingVerif { get; set; }
}