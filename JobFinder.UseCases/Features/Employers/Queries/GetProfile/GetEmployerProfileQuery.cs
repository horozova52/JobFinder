using JobFinder.Shared.DTOs.Employers;
using MediatR;

namespace JobFinder.UseCases.Features.Employers.Queries.GetProfile;

public record GetEmployerProfileQuery(string UserId)
    : IRequest<GetEmployerProfileResult>;

public record GetEmployerProfileResult(
    bool Success,
    string? Message,
    EmployerProfileDto? Data);