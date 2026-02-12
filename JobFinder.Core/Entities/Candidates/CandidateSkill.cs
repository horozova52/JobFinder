using JobFinder.Shared.Enums;
using JobFinder.Core.Entities.Common;

namespace JobFinder.Core.Entities.Candidates;

public class CandidateSkill
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }
    public int SkillId { get; set; }

    public SkillLevel Level { get; set; }  
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
