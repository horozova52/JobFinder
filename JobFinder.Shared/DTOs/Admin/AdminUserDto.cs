namespace JobFinder.Shared.DTOs.Admin;
public class AdminUserDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Role { get; set; } = "";
    public int UserType { get; set; }
    public bool IsActive { get; set; }
}