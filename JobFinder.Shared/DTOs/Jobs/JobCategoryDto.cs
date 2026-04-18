namespace JobFinder.Shared.DTOs.Jobs;

public class JobCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; 
    public int JobCount { get; set; }
}
