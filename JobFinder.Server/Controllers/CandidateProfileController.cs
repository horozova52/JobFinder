using AutoMapper;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Features.Candidates.Commands.UpdateProfile;
using JobFinder.UseCases.Features.Candidates.Queries.GetProfile;
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
public class CandidateProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CandidateProfileController(
        IMediator mediator,
        ApplicationDbContext db,
        IMapper mapper)
    {
        _mediator = mediator;
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var result = await _mediator.Send(new GetCandidateProfileQuery(userId));
        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var result = await _mediator.Send(new UpdateCandidateProfileCommand(userId, dto));
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpGet("by-id/{profileId:int}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetByProfileId(int profileId, CancellationToken ct)
    {
        var profile = await _db.CandidateProfiles
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Languages).ThenInclude(l => l.Language)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profileId, ct);

        if (profile == null) return NotFound();

        var dto = _mapper.Map<CandidateProfileDto>(profile);
        return Ok(dto);
    }
}