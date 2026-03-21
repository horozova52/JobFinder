using JobFinder.Shared.DTOs.Identity;
using JobFinder.Shared.Enums;
using MediatR;

namespace JobFinder.UseCases.Features.Identity.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName,
    UserType UserType) : IRequest<RegisterResult>;

public record RegisterResult(
    bool Success,
    string? Message,
    AuthResponseDto? Data);
