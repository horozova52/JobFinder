using JobFinder.Shared.Enums;

namespace JobFinder.Core.Entities.Candidates;

public class CandidateProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Headline { get; set; }
    public string? Location { get; set; }
    public string? Summary { get; set; }
    public bool IsCompleted { get; set; }
    public CandidateStatus Status { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LinkedIn { get; set; }
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public int? PreferredJobCategoryId { get; set; }
    public JobType? PreferredJobType { get; set; }

    public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    public ICollection<CandidateLanguage> Languages { get; set; } = new List<CandidateLanguage>();
}