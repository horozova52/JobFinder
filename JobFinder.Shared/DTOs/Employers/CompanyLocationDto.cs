namespace JobFinder.Shared.DTOs.Employers;

public class CompanyLocationDto
{
    public int Id { get; set; }
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
}
