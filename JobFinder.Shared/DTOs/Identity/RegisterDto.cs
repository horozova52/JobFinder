using JobFinder.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobFinder.Shared.DTOs.Identity;

public class RegisterDto
{
    [Required(ErrorMessage = "Email-ul este obligatoriu")]
    [EmailAddress(ErrorMessage = "Email invalid")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Parola este obligatorie")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie să aibă între 6 și 100 caractere")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Confirmare parolă obligatorie")]
    [Compare("Password", ErrorMessage = "Parolele nu coincid")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(200)]
    public string FullName { get; set; } = null!;

    [Required]
    public UserType UserType { get; set; }
}
