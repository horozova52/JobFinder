namespace JobFinder.Core.Entities.Jobs;

public class JobCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Icon { get; set; } = "";
    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
}
