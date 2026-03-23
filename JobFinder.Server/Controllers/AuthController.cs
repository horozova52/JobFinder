using JobFinder.Core.Entities.Identity;
using JobFinder.Shared.DTOs.Identity;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Identity.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        IMediator mediator, 
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.ConfirmPassword,
            request.FullName,
            request.UserType);

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        // Autentifică utilizatorul nou creat (setează cookie-ul de sesiune)
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            var token = await GenerateJwtToken(user);
            result.Data.Token = token;
        }

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { message = "Email sau parolă incorectă" });

        var result = await _signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized(new { message = "Email sau parolă incorectă" });

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            Token = "",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        return Ok(response);
    }

    [HttpPost("store-token")]
    public IActionResult StoreToken([FromBody] Dictionary<string, string> request)
    {
        // Token storage is handled by the client
        // This endpoint can be used for server-side token validation if needed
        return Ok(new { message = "Token received" });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("UserType", user.UserType.ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


