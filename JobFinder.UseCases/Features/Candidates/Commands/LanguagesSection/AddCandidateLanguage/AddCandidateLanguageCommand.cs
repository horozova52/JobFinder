using JobFinder.Shared.DTOs.Candidates;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.AddCandidateLanguage;

public record AddCandidateLanguageCommand(
    string UserId,
    int LanguageId,
    LanguageProficiencyLevel ProficiencyLevel
) : IRequest<Result<CandidateLanguageDto>>;