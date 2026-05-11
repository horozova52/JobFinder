using JobFinder.Core.Entities.Messaging;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _db;

    public ChatHub(ApplicationDbContext db)
    {
        _db = db;
    }

    private string UserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    public async Task JoinConversation(int conversationId)
    {
        var conversation = await _db.Conversations
            .Include(c => c.Application)
                .ThenInclude(a => a.CandidateProfile)
            .Include(c => c.Application)
                .ThenInclude(a => a.JobPosting)
                    .ThenInclude(j => j.EmployerProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return;

        var candidateUserId = conversation.Application.CandidateProfile.UserId;
        var employerUserId = conversation.Application.JobPosting.EmployerProfile.UserId;

        if (UserId != candidateUserId && UserId != employerUserId) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task SendMessage(int conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        var conversation = await _db.Conversations
            .Include(c => c.Application)
                .ThenInclude(a => a.CandidateProfile)
            .Include(c => c.Application)
                .ThenInclude(a => a.JobPosting)
                    .ThenInclude(j => j.EmployerProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conversation == null) return;
        var candidateUserId = conversation.Application.CandidateProfile.UserId;
        var employerUserId = conversation.Application.JobPosting.EmployerProfile.UserId;
        if (UserId != candidateUserId && UserId != employerUserId) return;
        var message = new Message
        {
            ConversationId = conversationId,
            SenderUserId = UserId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        _db.Messages.Add(message);
        conversation.LastMessageAt = message.SentAt;
        await _db.SaveChangesAsync();
        var senderName = UserId == candidateUserId
            ? $"{conversation.Application.CandidateProfile.FirstName} {conversation.Application.CandidateProfile.LastName}"
            : conversation.Application.JobPosting.EmployerProfile.CompanyName;
        var dto = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            SenderName = senderName,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            IsMine = false
        };
        await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("ReceiveMessage", dto);
    }

    public async Task MarkAsRead(int conversationId)
    {
        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderUserId != UserId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread)
            m.IsRead = true;

        await _db.SaveChangesAsync();
    }
}
