using JobFinder.UseCases.Features.Jobs.Queries.GetJobFeed;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class JobFeedController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobFeedController(IMediator mediator)
    {
        _mediator = mediator;
    }

   
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? jobType,
        [FromQuery] string? employmentType,
        [FromQuery] string? location,
        CancellationToken ct)
    {
       
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(new GetJobFeedQuery(
            UserId: userId,
            Search: search,
            Category: category,
            JobType: jobType,
            EmploymentType: employmentType,
            Location: location), ct);

        return Ok(result);
    }
}