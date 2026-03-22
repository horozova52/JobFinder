using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Common;
using JobFinder.UseCases.Contracts;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.AddCandidateLanguage;

public class AddCandidateLanguageHandler : IRequestHandler<AddCandidateLanguageCommand, Result<CandidateLanguageDto>>
{
    private readonly ICandidateLanguageRepository _langRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;

    public AddCandidateLanguageHandler(
        ICandidateLanguageRepository langRepo,
        ICandidateRepository candidateRepo,
        IMapper mapper)
    {
        _langRepo = langRepo;
        _candidateRepo = candidateRepo;
        _mapper = mapper;
    }

    public async Task<Result<CandidateLanguageDto>> Handle(
        AddCandidateLanguageCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _candidateRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile == null)
            return Result<CandidateLanguageDto>.Failure("Profilul nu a fost găsit");

        var duplicate = await _langRepo.AlreadyHasLanguageAsync(profile.Id, request.LanguageId, cancellationToken);
        if (duplicate)
            return Result<CandidateLanguageDto>.Failure("Limba este deja adăugată în profilul tău");

        var entity = new CandidateLanguage
        {
            CandidateProfileId = profile.Id,
            LanguageId = request.LanguageId,
            ProficiencyLevel = request.ProficiencyLevel,
        };

        var created = await _langRepo.CreateAsync(entity, cancellationToken);
        var dto = _mapper.Map<CandidateLanguageDto>(created);
        return Result<CandidateLanguageDto>.Success(dto);
    }
}