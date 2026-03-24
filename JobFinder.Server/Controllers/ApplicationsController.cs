using AutoMapper;
using JobFinder.Core.Entities.Applications;
using JobFinder.Core.Entities.Common;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Applications;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationRepository _applicationRepo;
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public ApplicationsController(
        IApplicationRepository applicationRepo,
        ApplicationDbContext db,
        IMapper mapper)
    {
        _applicationRepo = applicationRepo;
        _db = db;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] CreateApplicationDto dto, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (candidate == null)
            return BadRequest(new { message = "Profilul de candidat nu a fost găsit" });

        if (await _applicationRepo.AlreadyAppliedAsync(dto.JobPostingId, candidate.Id, ct))
            return Conflict(new { message = "Ai aplicat deja la acest job" });

        var application = new Application
        {
            JobPostingId = dto.JobPostingId,
            CandidateProfileId = candidate.Id,
            CoverLetter = dto.CoverLetter
        };

        var created = await _applicationRepo.CreateAsync(application, ct);

        // ── Notificare angajator — aplicare nouă ──────────────────────
        var jobWithEmployer = await _db.JobPostings
            .Include(j => j.EmployerProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == dto.JobPostingId, ct);

        if (jobWithEmployer?.EmployerProfile?.UserId is string employerUserId)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = employerUserId,
                Target = NotificationTarget.Employer,
                Type = NotificationType.NewApplicationReceived,
                Title = "Aplicare nouă la job",
                Message = $"{candidate.FirstName} {candidate.LastName} a aplicat la \"{jobWithEmployer.Title}\".",
                Link = $"/employer/applications/{created.Id}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new { id = created.Id, message = "Aplicare trimisă cu succes" });
    }
    [HttpGet("my")]
    public async Task<IActionResult> GetMyApplications(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (candidate == null)
            return Ok(Array.Empty<ApplicationDto>());

        var applications = await _applicationRepo.GetByCandidateIdAsync(candidate.Id, ct);
        var dtos = _mapper.Map<IEnumerable<ApplicationDto>>(applications);
        return Ok(dtos);
    }

    [HttpGet("job/{jobId:int}")]
    public async Task<IActionResult> GetJobApplications(int jobId, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var employer = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employer == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var job = await _db.JobPostings
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == employer.Id, ct);

        if (job == null)
            return NotFound(new { message = "Anunțul nu a fost găsit" });

        var applications = await _applicationRepo.GetByJobPostingIdAsync(jobId, ct);
        var dtos = _mapper.Map<IEnumerable<ApplicationDto>>(applications);
        return Ok(dtos);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
    int id,
    [FromBody] UpdateApplicationStatusDto dto,
    CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var employer = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employer == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var application = await _db.Applications
            .Include(a => a.JobPosting)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
            return NotFound(new { message = "Aplicarea nu a fost găsită" });

        if (application.JobPosting.EmployerProfileId != employer.Id)
            return Forbid();

        var newStatus = (ApplicationState)dto.Status;
        application.Status = newStatus;
        await _db.SaveChangesAsync(ct);

        // ── Notificare candidat — status schimbat ─────────────────────
        var statusLabel = newStatus switch
        {
            ApplicationState.InReview => "În analiză",
            ApplicationState.Interview => "Interviu programat",
            ApplicationState.Accepted => "Acceptat 🎉",
            ApplicationState.Rejected => "Respins",
            _ => newStatus.ToString()
        };

        var candidateUserId = application.CandidateProfile.UserId;
        if (!string.IsNullOrEmpty(candidateUserId))
        {
            _db.Notifications.Add(new Notification
            {
                UserId = candidateUserId,
                Target = NotificationTarget.Candidate,
                Type = NotificationType.ApplicationStatusChanged,
                Title = "Status aplicare actualizat",
                Message = $"Aplicarea ta la \"{application.JobPosting.Title}\" este acum: {statusLabel}.",
                Link = $"/candidate/applications/{application.Id}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new { message = "Status actualizat cu succes" });
    }

    [HttpGet("check/{jobId:int}")]
    public async Task<IActionResult> CheckAlreadyApplied(int jobId, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (candidate == null)
            return Ok(new { alreadyApplied = false });

        var applied = await _applicationRepo.AlreadyAppliedAsync(jobId, candidate.Id, ct);
        return Ok(new { alreadyApplied = applied });
    }

    [HttpGet("{id:int}/detail")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var employer = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employer == null) return BadRequest();

        var application = await _db.Applications
            .Include(a => a.JobPosting)
            .Include(a => a.CandidateProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.JobPosting.EmployerProfileId == employer.Id, ct);

        if (application == null) return NotFound();

        var dto = _mapper.Map<ApplicationDto>(application);
        return Ok(dto);
    }
}

public class UpdateApplicationStatusDto
{
    public int Status { get; set; }
}
