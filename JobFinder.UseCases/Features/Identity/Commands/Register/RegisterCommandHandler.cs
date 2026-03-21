using JobFinder.Core.Entities.Identity;
using JobFinder.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace JobFinder.UseCases.Features.Identity.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Validare parole
        if (request.Password != request.ConfirmPassword)
            return new RegisterResult(false, "Parolele nu coincid", null);

        // Verifică dacă emailul există deja
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return new RegisterResult(false, "Acest email este deja înregistrat", null);

        // Creare utilizator nou
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            UserType = request.UserType
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Code switch
            {
                "PasswordRequiresNonAlphanumeric" => "Parola trebuie să conțină cel puțin un caracter special (ex: !@#$%)",
                "PasswordRequiresDigit" => "Parola trebuie să conțină cel puțin o cifră (0-9)",
                "PasswordRequiresUpper" => "Parola trebuie să conțină cel puțin o literă mare (A-Z)",
                "PasswordRequiresLower" => "Parola trebuie să conțină cel puțin o literă mică (a-z)",
                "PasswordTooShort" => $"Parola trebuie să aibă minim {_userManager.Options.Password.RequiredLength} caractere",
                "PasswordRequiresUniqueChars" => "Parola trebuie să conțină mai multe caractere unice",
                "DuplicateUserName" => "Acest email este deja înregistrat",
                "DuplicateEmail" => "Acest email este deja înregistrat",
                "InvalidEmail" => "Adresa de email este invalidă",
                "InvalidUserName" => "Numele de utilizator conține caractere invalide",
                _ => e.Description
            });

            return new RegisterResult(false, string.Join("\n", errors), null);
        }

        var response = new Shared.DTOs.Identity.AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            Token = string.Empty,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        return new RegisterResult(true, "Înregistrare reușită", response);
    }
}
