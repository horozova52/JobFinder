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

    // New fields
    public string? FiscalCode { get; set; }
    public int? FoundedYear { get; set; }

    // Culture
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public string? Values { get; set; }
    public string? WorkEnvironment { get; set; }

    // Typed lists (serialized to JSON for storage)
    public List<string> Benefits { get; set; } = new();
    public List<RecruitmentStepDto> RecruitmentProcess { get; set; } = new();

    // Social & Contact
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}
