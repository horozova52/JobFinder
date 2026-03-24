namespace JobFinder.Shared.DTOs.Admin;
public class AdminEmployerVerificationDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? FiscalCode { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsVerified { get; set; }
    public string UserId { get; set; } = "";
}