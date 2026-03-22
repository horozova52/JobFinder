using JobFinder.Shared.DTOs.Candidates;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.AddCandidateSkill;

public record AddCandidateSkillCommand(
    string UserId,
    string SkillName,
    SkillLevel Level
) : IRequest<Result<CandidateSkillDto>>;