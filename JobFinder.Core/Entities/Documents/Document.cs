using JobFinder.Shared.Enums;
using System.Xml.Linq;

namespace JobFinder.Core.Entities.Documents;

public class Document
{
    public int Id { get; set; }

    public string OwnerId { get; set; } = null!;   
    public string OwnerType { get; set; } = "Candidate"; 

    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string Url { get; set; } = null!;         

    public DateTime CreatedAt { get; set; }
}
