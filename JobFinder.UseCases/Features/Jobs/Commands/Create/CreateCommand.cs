using JobFinder.Shared.DTOs.Jobs;
using JobFinder.Shared.Enums;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Commands.Create;

public record CreateCommand(
    int EmployerProfileId,
    string Title,
    string Description,
    string? Requirements,
    string? Responsibilities,
    string? Location,
    JobType JobType,
    EmploymentType EmploymentType,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    bool IsSalaryNegotiable
) : IRequest<JobPostingDto>;