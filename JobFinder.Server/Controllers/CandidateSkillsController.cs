using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.AddCandidateSkill;
using JobFinder.UseCases.Features.Candidates.Commands.SkillsSection.DeleteCandidateSkill;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class CandidateSkillsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidateSkillsController(IMediator mediator)
    {
        _mediator = mediator;
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