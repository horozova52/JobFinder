using JobFinder.Shared.DTOs.Employers;
using JobFinder.UseCases.Features.Employers.Commands.UpdateProfile;
using JobFinder.UseCases.Features.Employers.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/employer/profile")]
[Authorize(Roles = "Employer")]
[IgnoreAntiforgeryToken]
public class EmployerProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployerProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Utilizator neidentificat" });

        var result = await _mediator.Send(new GetEmployerProfileQuery(userId), ct);

        if (!result.Success)
            return NotFound(new { message = result.Message });

        return Ok(result.Data);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateEmployerProfileDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Utilizator neidentificat" });

        var result = await _mediator.Send(
            new UpdateEmployerProfileCommand(userId, dto), ct);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}