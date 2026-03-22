namespace JobFinder.Shared.DTOs.Candidates;

public class EducationDto
{
    public int Id { get; set; }
    public string Institution { get; set; } = null!;
    public string Degree { get; set; } = null!;
    public string? FieldOfStudy { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public bool IsCurrent => EndDate == null;
}
