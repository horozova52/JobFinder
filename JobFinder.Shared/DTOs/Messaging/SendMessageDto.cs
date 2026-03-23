namespace JobFinder.Shared.DTOs.Messaging;

public class SendMessageDto
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = "";
}
