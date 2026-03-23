using JobFinder.Core.Entities.Applications;

namespace JobFinder.Core.Entities.Messaging;

public class Conversation
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }

    public Application Application { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
