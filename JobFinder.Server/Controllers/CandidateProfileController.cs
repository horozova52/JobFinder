using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Features.Candidates.Commands.UpdateProfile;
using JobFinder.UseCases.Features.Candidates.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class CandidateProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidateProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returnează profilul candidatului autentificat.
    /// Dacă profilul nu există, îl creează automat.
    /// </summary>
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

    /// <summary>
    /// Actualizează datele personale ale candidatului autentificat.
    /// </summary>
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
}