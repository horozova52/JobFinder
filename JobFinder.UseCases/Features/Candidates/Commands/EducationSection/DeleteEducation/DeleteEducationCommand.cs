using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.EducationSection.DeleteEducation;

public record DeleteEducationCommand(
    int Id,
    string UserId
) : IRequest<Result<bool>>;

public record DeleteEducationResult(
    bool Success,
    string? Message);