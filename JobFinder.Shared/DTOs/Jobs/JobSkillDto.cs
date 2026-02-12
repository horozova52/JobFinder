using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Jobs;

public class JobSkillDto
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = null!;
    public SkillLevel RequiredLevel { get; set; }
}
