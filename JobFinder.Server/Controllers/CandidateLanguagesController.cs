using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.AddCandidateLanguage;
using JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.DeleteCandidateLanguage;
using JobFinder.UseCases.Features.Candidates.Commands.LanguagesSection.UpdateCandidateLanguage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class CandidateLanguagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidateLanguagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddLanguageRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(
            new AddCandidateLanguageCommand(userId, request.LanguageId, request.ProficiencyLevel));

        if (!result.IsSuccess) return BadRequest(new { message = result.Error });
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLanguageRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(
            new UpdateCandidateLanguageCommand(id, userId, request.ProficiencyLevel));

        if (!result.IsSuccess) return BadRequest(new { message = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new DeleteCandidateLanguageCommand(id, userId));

        if (!result.IsSuccess) return BadRequest(new { message = result.Error });
        return Ok(new { message = "Limba a fost ștearsă cu succes" });
    }

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

public record AddLanguageRequest(int LanguageId, LanguageProficiencyLevel ProficiencyLevel);
public record UpdateLanguageRequest(LanguageProficiencyLevel ProficiencyLevel);