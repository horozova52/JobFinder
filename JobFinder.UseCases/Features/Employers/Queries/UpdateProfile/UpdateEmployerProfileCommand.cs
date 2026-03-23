using JobFinder.Shared.DTOs.Employers;
using MediatR;

namespace JobFinder.UseCases.Features.Employers.Commands.UpdateProfile;

public record UpdateEmployerProfileCommand(
    string UserId,
    UpdateEmployerProfileDto Dto)
    : IRequest<UpdateEmployerProfileResult>;

public record UpdateEmployerProfileResult(
    bool Success,
    string? Message,
    EmployerProfileDto? Data);