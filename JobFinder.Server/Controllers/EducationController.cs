using JobFinder.Shared.DTOs.Candidates;
using JobFinder.UseCases.Features.Candidates.Commands.EducationSection.AddEducation;
using JobFinder.UseCases.Features.Candidates.Commands.EducationSection.DeleteEducation;
using JobFinder.UseCases.Features.Candidates.Commands.EducationSection.UpdateEducation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class EducationController : ControllerBase
{
    private readonly IMediator _mediator;

    public EducationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] EducationDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var command = new AddEducationCommand(
            UserId: userId,
            Institution: dto.Institution,
            Degree: dto.Degree,
            FieldOfStudy: dto.FieldOfStudy,
            StartDate: dto.StartDate,
            EndDate: dto.EndDate,
            Description: dto.Description);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EducationDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var command = new UpdateEducationCommand(
            Id: id,
            UserId: userId,
            Institution: dto.Institution,
            Degree: dto.Degree,
            FieldOfStudy: dto.FieldOfStudy,
            StartDate: dto.StartDate,
            EndDate: dto.EndDate,
            Description: dto.Description);

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

        var result = await _mediator.Send(new DeleteEducationCommand(id, userId));

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(new { message = "Educația a fost ștearsă cu succes" });
    }

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}