namespace JobFinder.Core.Entities.Messaging;

public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderUserId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
