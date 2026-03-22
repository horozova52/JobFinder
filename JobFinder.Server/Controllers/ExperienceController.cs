using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.AddExperience;
using JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.DeleteExperience;
using JobFinder.UseCases.Features.Candidates.Commands.ExperienceSection.UpdateExperience;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class ExperienceController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExperienceController(IMediator mediator)
    {
        _mediator = mediator;
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

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}