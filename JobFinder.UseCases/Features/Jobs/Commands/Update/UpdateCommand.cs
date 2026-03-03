using JobFinder.Shared.Enums;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Commands.Update;

public record UpdateCommand(
    int Id,
    string Title,
    string Description,
    string? Requirements,
    string? Responsibilities,
    string? Location,
    JobType JobType,
    EmploymentType EmploymentType,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    bool IsSalaryNegotiable,
    JobStatus Status
) : IRequest<bool>;