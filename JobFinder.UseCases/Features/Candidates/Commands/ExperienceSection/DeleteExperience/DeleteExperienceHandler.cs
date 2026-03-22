using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.DeleteExperience;

public class DeleteExperienceHandler : IRequestHandler<DeleteExperienceCommand, Result<bool>>
{
    private readonly IExperienceRepository _experienceRepo;
    private readonly ICandidateRepository _candidateRepo;

    public DeleteExperienceHandler(
        IExperienceRepository experienceRepo,
        ICandidateRepository candidateRepo)
    {
        _experienceRepo = experienceRepo;
        _candidateRepo = candidateRepo;
    }

    public async Task<Result<bool>> Handle(DeleteExperienceCommand request, CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<bool>.Failure("Profilul nu a fost găsit");

        var exists = await _experienceRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<bool>.Failure("Înregistrarea nu a fost găsită");

        await _experienceRepo.DeleteAsync(request.Id, cancellationToken);
        return Result<bool>.Success(true);
    }
}