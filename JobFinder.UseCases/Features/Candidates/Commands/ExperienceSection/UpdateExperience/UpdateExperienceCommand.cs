using JobFinder.Shared.DTOs.Candidates;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.UpdateExperience;

public record UpdateExperienceCommand(
    int Id,
    string UserId,
    string CompanyName,
    string Position,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    string? Description,
    string? Location,
    EmploymentType? EmploymentType
) : IRequest<Result<ExperienceDto>>;