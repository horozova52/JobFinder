namespace JobFinder.Shared.DTOs.Employers;

public class EmployerProfileDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? ShortTitle { get; set; }
    public string? CompanySize { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsVerified { get; set; }
    public List<CompanyLocationDto> Locations { get; set; } = new();
}
