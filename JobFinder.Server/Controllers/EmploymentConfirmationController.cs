using JobFinder.Core.Entities.Applications;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Application;
using JobFinder.Shared.DTOs.Applications;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/employment-confirmations")]
[Authorize]
[IgnoreAntiforgeryToken]
public class EmploymentConfirmationController : ControllerBase
{
    private readonly IEmploymentConfirmationRepository _repo;
    private readonly ApplicationDbContext _db;

    public EmploymentConfirmationController(
        IEmploymentConfirmationRepository repo,
        ApplicationDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    // Candidatul confirmă că s-a angajat
    [HttpPost("{applicationId:int}/confirm")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Confirm(int applicationId, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (candidate == null)
            return BadRequest(new { message = "Profilul de candidat nu a fost găsit" });

        var application = await _db.Applications
            .Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a =>
                a.Id == applicationId &&
                a.CandidateProfileId == candidate.Id, ct);

        if (application == null)
            return NotFound(new { message = "Aplicarea nu a fost găsită" });

        if (application.Status != ApplicationState.Accepted)
            return BadRequest(new { message = "Poți confirma angajarea doar pentru aplicările acceptate" });

        if (await _repo.ExistsForApplicationAsync(applicationId, ct))
            return Conflict(new { message = "Ai confirmat deja angajarea pentru această aplicare" });

        var confirmation = new EmploymentConfirmation
        {
            ApplicationId = applicationId,
            CandidateProfileId = candidate.Id,
            EmployerProfileId = application.JobPosting.EmployerProfileId,
            ConfirmedAt = DateTime.UtcNow,
            Status = EmploymentConfirmationStatus.PendingValidation,
            AddToExperience = true,
        };

        await _repo.CreateAsync(confirmation, ct);

        return Ok(new { message = "Angajarea a fost confirmată. Angajatorul va valida în curând." });
    }

    // Angajatorul vede confirmările în așteptare
    [HttpGet("pending")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var employer = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employer == null)
            return Ok(Array.Empty<EmploymentConfirmationDto>());

        var confirmations = await _repo.GetPendingByEmployerAsync(employer.Id, ct);

        var dtos = confirmations.Select(c => new EmploymentConfirmationDto
        {
            Id = c.Id,
            ApplicationId = c.ApplicationId,
            JobTitle = c.Application.JobPosting.Title,
            CandidateFullName = $"{c.Application.CandidateProfile.FirstName} {c.Application.CandidateProfile.LastName}",
            CompanyName = employer.CompanyName ?? "",
            ConfirmedAt = c.ConfirmedAt,
            ValidatedAt = c.ValidatedAt,
            Status = c.Status,
        });

        return Ok(dtos);
    }

    // Angajatorul validează sau respinge confirmarea
    [HttpPut("{id:int}/validate")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Validate(
        int id,
        [FromBody] ValidateConfirmationDto dto,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var employer = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);

        if (employer == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var confirmation = await _db.EmploymentConfirmations
            .Include(c => c.Application)
                .ThenInclude(a => a.JobPosting)
            .Include(c => c.Application)
                .ThenInclude(a => a.CandidateProfile)
                    .ThenInclude(cp => cp.Experiences)
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.EmployerProfileId == employer.Id, ct);

        if (confirmation == null)
            return NotFound(new { message = "Confirmarea nu a fost găsită" });

        if (confirmation.Status != EmploymentConfirmationStatus.PendingValidation)
            return BadRequest(new { message = "Confirmarea a fost deja procesată" });

        if (dto.Approved)
        {
            confirmation.Status = EmploymentConfirmationStatus.Validated;
            confirmation.ValidatedAt = DateTime.UtcNow;

            // Adăugăm automat în Experience dacă nu există deja
            if (confirmation.AddToExperience)
            {
                var job = confirmation.Application.JobPosting;
                var profile = confirmation.Application.CandidateProfile;
                var alreadyExists = profile.Experiences.Any(e =>
                    e.CompanyName == job.EmployerProfile?.CompanyName &&
                    e.Position == job.Title);

                if (!alreadyExists)
                {
                    var experience = new Experience
                    {
                        CandidateProfileId = profile.Id,
                        CompanyName = employer.CompanyName ?? job.Title,
                        Position = job.Title,
                        StartDate = confirmation.Application.AppliedAt,
                        IsCurrent = true,
                        Description = $"Angajat prin platforma RecruitBoard. " +
                                             $"Validat de angajator pe {DateTime.UtcNow:dd.MM.yyyy}.",
                        EmploymentType = job.EmploymentType,
                        Location = job.Location,
                    };
                    _db.Experiences.Add(experience);
                }
            }
        }
        else
        {
            confirmation.Status = EmploymentConfirmationStatus.Rejected;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = dto.Approved
                ? "Angajarea a fost validată. Experiența a fost adăugată automat în profilul candidatului."
                : "Confirmarea a fost respinsă."
        });
    }

    // Candidatul verifică statusul confirmării pentru o aplicare
    [HttpGet("status/{applicationId:int}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetStatus(int applicationId, CancellationToken ct)
    {
        var confirmation = await _repo.GetByApplicationIdAsync(applicationId, ct);
        if (confirmation == null)
            return Ok(new { exists = false, status = (int?)null });

        return Ok(new
        {
            exists = true,
            status = (int)confirmation.Status,
            validatedAt = confirmation.ValidatedAt,
        });
    }
    [HttpGet("by-application/{applicationId:int}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetByApplication(int applicationId, CancellationToken ct)
    {
        var confirmation = await _db.EmploymentConfirmations
            .FirstOrDefaultAsync(c => c.ApplicationId == applicationId, ct);

        if (confirmation == null) return NotFound();

        return Ok(new EmploymentConfirmationDto
        {
            Id = confirmation.Id,
            ApplicationId = confirmation.ApplicationId,
            ConfirmedAt = confirmation.ConfirmedAt,
            ValidatedAt = confirmation.ValidatedAt,
            Status = confirmation.Status,
        });
    }
}

public class ValidateConfirmationDto
{
    public bool Approved { get; set; }
}