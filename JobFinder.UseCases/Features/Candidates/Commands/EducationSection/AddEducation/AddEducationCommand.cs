using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.EducationSection.AddEducation;

public record AddEducationCommand(
    string UserId,
    string Institution,
    string Degree,
    string? FieldOfStudy,
    DateTime StartDate,
    DateTime? EndDate,
    string? Description
) : IRequest<Result<EducationDto>>;
