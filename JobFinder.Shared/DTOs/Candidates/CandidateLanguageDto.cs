using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Candidates;

public class CandidateLanguageDto
{
    public int Id { get; set; }
    public int LanguageId { get; set; }
    public string LanguageName { get; set; } = null!;
    public LanguageProficiencyLevel ProficiencyLevel { get; set; }
}
