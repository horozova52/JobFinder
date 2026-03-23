namespace JobFinder.Core.Entities.Employers;

public class EmployerProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }

    public bool IsVerified { get; set; }
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

    // JSON stored
    public string? BenefitsJson { get; set; }
    public string? RecruitmentProcessJson { get; set; }

    // Social & Contact
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public ICollection<CompanyLocation> Locations { get; set; } = new List<CompanyLocation>();
}
