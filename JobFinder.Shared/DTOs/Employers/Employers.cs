namespace JobFinder.Shared.DTOs.Employers;

public class EmployerProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }

    public bool IsVerified { get; set; }

    public List<CompanyLocationDto> Locations { get; set; } = [];
}
