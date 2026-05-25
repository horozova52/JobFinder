using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.AddExperience;
using JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.DeleteExperience;
using JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.UpdateExperience;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class ExperienceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _db;
    public ExperienceController(IMediator mediator, ApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ExperienceDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var command = new AddExperienceCommand(
            UserId: userId,
            CompanyName: dto.CompanyName,
            Position: dto.Position,
            StartDate: dto.StartDate,
            EndDate: dto.EndDate,
            IsCurrent: dto.IsCurrent,
            Description: dto.Description,
            Location: dto.Location,
            EmploymentType: dto.EmploymentType);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExperienceDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var command = new UpdateExperienceCommand(
            Id: id,
            UserId: userId,
            CompanyName: dto.CompanyName,
            Position: dto.Position,
            StartDate: dto.StartDate,
            EndDate: dto.EndDate,
            IsCurrent: dto.IsCurrent,
            Description: dto.Description,
            Location: dto.Location,
            EmploymentType: dto.EmploymentType);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var result = await _mediator.Send(new DeleteExperienceCommand(id, userId));

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(new { message = "Experiența a fost ștearsă cu succes" });
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> ConfirmEmployment(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var experience = await _db.Experiences
            .Include(e => e.CandidateProfile)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (experience == null)
            return NotFound(new { message = "Experiența nu a fost găsită" });

        if (experience.CandidateProfile.UserId != userId)
            return Forbid();

        if (experience.Status != ExperienceStatus.PendingConfirmation)
            return BadRequest(new { message = "Această experiență nu așteaptă confirmare." });

        experience.Status = ExperienceStatus.Active;
        experience.IsCurrent = true;
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Angajare confirmată." });
    }

    // ── 5.4 Candidatul demisionează ───────────────────────────────────
    [HttpPut("{id:int}/resign")]
    public async Task<IActionResult> Resign(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var experience = await _db.Experiences
            .Include(e => e.CandidateProfile)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (experience == null)
            return NotFound(new { message = "Experiența nu a fost găsită" });

        if (experience.CandidateProfile.UserId != userId)
            return Forbid();

        if (experience.Status != ExperienceStatus.Active)
            return BadRequest(new { message = "Poți demisiona doar dintr-o angajare activă." });

        experience.Status = ExperienceStatus.Ended;
        experience.IsCurrent = false;
        experience.EndDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Demisie înregistrată. Experiența rămâne în profil." });
    }
    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}