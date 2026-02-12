using JobFinder.Shared.Enums;
using JobFinder.Core.Entities.Common;

namespace JobFinder.Core.Entities.Candidates;

public class CandidateLanguage
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }
    public int LanguageId { get; set; }

    public LanguageProficiencyLevel ProficiencyLevel { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
