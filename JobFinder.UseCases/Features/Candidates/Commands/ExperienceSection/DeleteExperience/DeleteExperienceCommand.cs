using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.DeleteExperience;

public record DeleteExperienceCommand(
    int Id,
    string UserId
) : IRequest<Result<bool>>;