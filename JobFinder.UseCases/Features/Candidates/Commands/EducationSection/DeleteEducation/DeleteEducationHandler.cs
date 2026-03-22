using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.EducationSection.DeleteEducation;

public class DeleteEducationHandler : IRequestHandler<DeleteEducationCommand, Result<bool>>
{
    private readonly IEducationRepository _educationRepo;
    private readonly ICandidateRepository _candidateRepo;

    public DeleteEducationHandler(
        IEducationRepository educationRepo,
        ICandidateRepository candidateRepo)
    {
        _educationRepo = educationRepo;
        _candidateRepo = candidateRepo;
    }

    public async Task<Result<bool>> Handle(DeleteEducationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<bool>.Failure("Profilul nu a fost găsit");

        var exists = await _educationRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<bool>.Failure("Înregistrarea nu a fost găsită");

        await _educationRepo.DeleteAsync(request.Id, cancellationToken);
        return Result<bool>.Success(true);
    }
}