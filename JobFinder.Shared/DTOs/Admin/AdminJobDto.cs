namespace JobFinder.Shared.DTOs.Admin;

public class AdminJobDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Location { get; set; }
}