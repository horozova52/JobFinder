namespace JobFinder.Core.Entities.Employers;

public class CompanyLocation
{
    public int Id { get; set; }

    public int EmployerProfileId { get; set; }
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;

    public EmployerProfile EmployerProfile { get; set; } = null!;
}
