using JobFinder.Shared.DTOs.Candidates;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Common;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.UpdateCandidateLanguage;

public record UpdateCandidateLanguageCommand(
    int Id,
    string UserId,
    LanguageProficiencyLevel ProficiencyLevel
) : IRequest<Result<CandidateLanguageDto>>;