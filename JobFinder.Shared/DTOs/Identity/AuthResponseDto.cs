using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Identity;

public class AuthResponseDto
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserType UserType { get; set; }

    public string Token { get; set; } = null!;     
    public DateTime ExpiresAt { get; set; }
}
