namespace JobFinder.Core.Entities.Documents;

public class DocumentTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
}
