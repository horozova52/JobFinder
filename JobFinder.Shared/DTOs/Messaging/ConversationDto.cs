namespace JobFinder.Shared.DTOs.Messaging;

public class ConversationDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string OtherPartyName { get; set; } = "";
    public string? JobTitle { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
