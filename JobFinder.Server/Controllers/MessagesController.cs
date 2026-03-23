using JobFinder.Core.Entities.Messaging;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MessagesController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var candidate = await _db.CandidateProfiles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        var employer = await _db.EmployerProfiles.AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        var query = _db.Conversations
            .Include(c => c.Application)
                .ThenInclude(a => a.CandidateProfile)
            .Include(c => c.Application)
                .ThenInclude(a => a.JobPosting)
                    .ThenInclude(j => j.EmployerProfile)
            .Include(c => c.Messages)
            .AsNoTracking();

        if (candidate != null)
            query = query.Where(c => c.Application.CandidateProfileId == candidate.Id);
        else if (employer != null)
            query = query.Where(c => c.Application.JobPosting.EmployerProfileId == employer.Id);
        else
            return Ok(Array.Empty<ConversationDto>());

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(ct);

        var dtos = conversations.Select(c =>
        {
            var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unread = c.Messages.Count(m => m.SenderUserId != userId && !m.IsRead);

            string otherPartyName;
            if (candidate != null)
                otherPartyName = c.Application.JobPosting.EmployerProfile.CompanyName;
            else
                otherPartyName = $"{c.Application.CandidateProfile.FirstName} {c.Application.CandidateProfile.LastName}";

            return new ConversationDto
            {
                Id = c.Id,
                ApplicationId = c.ApplicationId,
                OtherPartyName = otherPartyName,
                JobTitle = c.Application.JobPosting.Title,
                LastMessage = lastMsg?.Content,
                LastMessageAt = lastMsg?.SentAt ?? c.CreatedAt,
                UnreadCount = unread
            };
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken ct)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var conversation = await _db.Conversations
            .Include(c => c.Application)
                .ThenInclude(a => a.CandidateProfile)
            .Include(c => c.Application)
                .ThenInclude(a => a.JobPosting)
                    .ThenInclude(j => j.EmployerProfile)
            .Include(c => c.Messages)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (conversation == null)
            return NotFound(new { message = "Conversația nu a fost găsită" });

        var candidateUserId = conversation.Application.CandidateProfile.UserId;
        var employerUserId = conversation.Application.JobPosting.EmployerProfile.UserId;

        if (userId != candidateUserId && userId != employerUserId)
            return Forbid();

        var dtos = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m =>
            {
                var senderName = m.SenderUserId == candidateUserId
                    ? $"{conversation.Application.CandidateProfile.FirstName} {conversation.Application.CandidateProfile.LastName}"
                    : conversation.Application.JobPosting.EmployerProfile.CompanyName;

                return new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderUserId = m.SenderUserId,
                    SenderName = senderName,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    IsMine = m.SenderUserId == userId
                };
            }).ToList();

        return Ok(dtos);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto, CancellationToken ct)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var application = await _db.Applications
            .Include(a => a.CandidateProfile)
            .Include(a => a.JobPosting)
                .ThenInclude(j => j.EmployerProfile)
            .FirstOrDefaultAsync(a => a.Id == dto.ApplicationId, ct);

        if (application == null)
            return NotFound(new { message = "Aplicarea nu a fost găsită" });

        var candidateUserId = application.CandidateProfile.UserId;
        var employerUserId = application.JobPosting.EmployerProfile.UserId;

        if (userId != candidateUserId && userId != employerUserId)
            return Forbid();

        var existing = await _db.Conversations
            .FirstOrDefaultAsync(c => c.ApplicationId == dto.ApplicationId, ct);

        if (existing != null)
            return Ok(new { id = existing.Id });

        var conversation = new Conversation
        {
            ApplicationId = dto.ApplicationId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id = conversation.Id });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto, CancellationToken ct)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "Mesajul nu poate fi gol" });

        var conversation = await _db.Conversations
            .Include(c => c.Application)
                .ThenInclude(a => a.CandidateProfile)
            .Include(c => c.Application)
                .ThenInclude(a => a.JobPosting)
                    .ThenInclude(j => j.EmployerProfile)
            .FirstOrDefaultAsync(c => c.Id == dto.ConversationId, ct);

        if (conversation == null)
            return NotFound(new { message = "Conversația nu a fost găsită" });

        var candidateUserId = conversation.Application.CandidateProfile.UserId;
        var employerUserId = conversation.Application.JobPosting.EmployerProfile.UserId;

        if (userId != candidateUserId && userId != employerUserId)
            return Forbid();

        var message = new Message
        {
            ConversationId = dto.ConversationId,
            SenderUserId = userId,
            Content = dto.Content.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.Messages.Add(message);
        conversation.LastMessageAt = message.SentAt;
        await _db.SaveChangesAsync(ct);

        var senderName = userId == candidateUserId
            ? $"{conversation.Application.CandidateProfile.FirstName} {conversation.Application.CandidateProfile.LastName}"
            : conversation.Application.JobPosting.EmployerProfile.CompanyName;

        var result = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            SenderName = senderName,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            IsMine = true
        };

        return Ok(result);
    }

    [HttpPut("read/{conversationId:int}")]
    public async Task<IActionResult> MarkAsRead(int conversationId, CancellationToken ct)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderUserId != userId && !m.IsRead)
            .ToListAsync(ct);

        foreach (var m in unread)
            m.IsRead = true;

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Mesajele au fost marcate ca citite" });
    }
}

public class CreateConversationDto
{
    public int ApplicationId { get; set; }
}
