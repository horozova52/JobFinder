using JobFinder.Shared.Enums;

namespace JobFinder.Shared.DTOs.Candidates;

public class CandidateProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Headline { get; set; }
    public string? Location { get; set; }
    public CandidateStatus Status { get; set; }
    public bool IsCompleted { get; set; }
    public string? Summary { get; set; }

    public List<ExperienceDto> Experiences { get; set; } = [];
    public List<EducationDto> Educations { get; set; } = [];
    public List<CandidateSkillDto> Skills { get; set; } = [];
    public List<CertificationDto> Certifications { get; set; } = [];
    public List<CandidateLanguageDto> Languages { get; set; } = [];
}
