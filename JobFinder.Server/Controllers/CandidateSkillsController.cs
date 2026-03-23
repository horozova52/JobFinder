using AutoMapper;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.AddCandidateSkill;
using JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.DeleteCandidateSkill;
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
public class CandidateSkillsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CandidateSkillsController(IMediator mediator, ApplicationDbContext db, IMapper mapper)
    {
        _mediator = mediator;
        _db = db;
        _mapper = mapper;
    }

    [HttpGet("candidate/{candidateId:int}")]
    public async Task<IActionResult> GetByCandidateId(int candidateId, CancellationToken ct)
    {
        var skills = await _db.CandidateSkills
            .Include(cs => cs.Skill)
            .Where(cs => cs.CandidateProfileId == candidateId)
            .AsNoTracking()
            .ToListAsync(ct);

        var dtos = _mapper.Map<List<CandidateSkillDto>>(skills);
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddSkillRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        if (string.IsNullOrWhiteSpace(request.SkillName))
            return BadRequest(new { message = "Numele competenței este obligatoriu" });

        var command = new AddCandidateSkillCommand(
            UserId: userId,
            SkillName: request.SkillName,
            Level: request.Level);

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

        var result = await _mediator.Send(new DeleteCandidateSkillCommand(id, userId));

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(new { message = "Competența a fost ștearsă cu succes" });
    }

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

public record AddSkillRequest(string SkillName, SkillLevel Level);