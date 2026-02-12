using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Candidates;

public class CandidateSkillDto
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = null!;
    public SkillLevel Level { get; set; }
}
