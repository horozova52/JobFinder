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

    public ICollection<CompanyLocation> Locations { get; set; } = new List<CompanyLocation>();
}
