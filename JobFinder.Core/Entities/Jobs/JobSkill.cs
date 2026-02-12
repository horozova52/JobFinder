using JobFinder.Shared.Enums;
using JobFinder.Core.Entities.Common;

namespace JobFinder.Core.Entities.Jobs;

public class JobSkill
{
    public int Id { get; set; }

    public int JobPostingId { get; set; }
    public int SkillId { get; set; }

    public SkillLevel RequiredLevel { get; set; }

    public JobPosting JobPosting { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
