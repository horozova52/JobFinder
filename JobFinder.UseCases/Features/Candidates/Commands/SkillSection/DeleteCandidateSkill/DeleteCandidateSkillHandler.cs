using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.DeleteCandidateSkill;

public class DeleteCandidateSkillHandler : IRequestHandler<DeleteCandidateSkillCommand, Result<bool>>
{
    private readonly ICandidateSkillRepository _skillRepo;
    private readonly ICandidateRepository _candidateRepo;

    public DeleteCandidateSkillHandler(
        ICandidateSkillRepository skillRepo,
        ICandidateRepository candidateRepo)
    {
        _skillRepo = skillRepo;
        _candidateRepo = candidateRepo;
    }

    public async Task<Result<bool>> Handle(DeleteCandidateSkillCommand request, CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<bool>.Failure("Profilul nu a fost găsit");

        var exists = await _skillRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<bool>.Failure("Competența nu a fost găsită");

        await _skillRepo.DeleteAsync(request.Id, cancellationToken);
        return Result<bool>.Success(true);
    }
}