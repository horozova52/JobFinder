using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.DeleteCandidateLanguage;

public record DeleteCandidateLanguageCommand(
    int Id,
    string UserId
) : IRequest<Result<bool>>;

public class DeleteCandidateLanguageHandler : IRequestHandler<DeleteCandidateLanguageCommand, Result<bool>>
{
    private readonly ICandidateLanguageRepository _langRepo;
    private readonly ICandidateRepository _candidateRepo;

    public DeleteCandidateLanguageHandler(
        ICandidateLanguageRepository langRepo,
        ICandidateRepository candidateRepo)
    {
        _langRepo = langRepo;
        _candidateRepo = candidateRepo;
    }

    public async Task<Result<bool>> Handle(
        DeleteCandidateLanguageCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<bool>.Failure("Profilul nu a fost găsit");

        var exists = await _langRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<bool>.Failure("Limba nu a fost găsită");

        await _langRepo.DeleteAsync(request.Id, cancellationToken);
        return Result<bool>.Success(true);
    }
}