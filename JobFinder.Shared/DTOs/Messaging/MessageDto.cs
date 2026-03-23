namespace JobFinder.Shared.DTOs.Messaging;

public class MessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderUserId { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsMine { get; set; }
}
