using JobFinder.Shared.DTOs.Candidates;

namespace JobFinder.Client.Pages.Candidate.Sections;

/// <summary>
/// View Model pentru formularul de educație.
/// Separat de EducationDto pentru că formularul lucrează cu ani (int)
/// în loc de DateTime, simplificând binding-ul în UI.
/// </summary>
public class EduDraft
{
    public string Institution { get; set; } = "";
    public string Degree { get; set; } = "";
    public string FieldOfStudy { get; set; } = "";
    public int StartYear { get; set; } = DateTime.Now.Year;
    public int? EndYear { get; set; }
    public bool IsCurrent { get; set; } = false;
    public string Description { get; set; } = "";

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Institution) &&
        !string.IsNullOrWhiteSpace(Degree) &&
        StartYear > 0;

    public EducationDto ToDto() => new()
    {
        Institution = Institution,
        Degree = Degree,
        FieldOfStudy = FieldOfStudy,
        StartDate = new DateTime(StartYear, 1, 1),
        EndDate = IsCurrent || !EndYear.HasValue
                           ? null
                           : new DateTime(EndYear.Value, 12, 31),
        Description = Description,
    };
}