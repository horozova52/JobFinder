using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.DeleteCandidateSkill;

public record DeleteCandidateSkillCommand(
    int Id,
    string UserId
) : IRequest<Result<bool>>;