using AutoMapper;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.UpdateCandidateLanguage;

public class UpdateCandidateLanguageHandler : IRequestHandler<UpdateCandidateLanguageCommand, Result<CandidateLanguageDto>>
{
    private readonly ICandidateLanguageRepository _langRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public UpdateCandidateLanguageHandler(
        ICandidateLanguageRepository langRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _langRepo = langRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<CandidateLanguageDto>> Handle(
        UpdateCandidateLanguageCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<CandidateLanguageDto>.Failure("Profilul nu a fost găsit");

        var exists = await _langRepo.ExistsAsync(request.Id, profile.Id, cancellationToken);
        if (!exists)
            return Result<CandidateLanguageDto>.Failure("Limba nu a fost găsită");

        var entity = await _langRepo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            return Result<CandidateLanguageDto>.Failure("Limba nu a fost găsită");

        // Singura proprietate care se poate schimba — nivelul CEFR
        entity.ProficiencyLevel = request.ProficiencyLevel;

        await _langRepo.UpdateAsync(entity, cancellationToken);

        // Re-fetch cu Language inclus pentru LanguageName
        var updated = await _langRepo.GetByIdAsync(entity.Id, cancellationToken);
        var dto = _mapper.Map<CandidateLanguageDto>(updated);
        return Result<CandidateLanguageDto>.Success(dto);
    }
}