namespace JobFinder.Shared.DTOs.Employers;

public class UpdateEmployerProfileDto
{
    public string CompanyName { get; set; } = "";
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? ShortTitle { get; set; }
    public string? CompanySize { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? LogoUrl { get; set; }
}