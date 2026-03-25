using JobFinder.UseCases.Features.Jobs.Queries.GetJobFeed;
using JobFinder.UseCases.Contracts;
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
    private readonly IJobFeedRepository _jobFeedRepo;

    public JobFeedController(IMediator mediator, IJobFeedRepository jobFeedRepo)
    {
        _mediator = mediator;
        _jobFeedRepo = jobFeedRepo;
    }

    [HttpGet("count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCount(CancellationToken ct)
    {
        var count = await _jobFeedRepo.GetPublishedJobsCountAsync(ct);
        return Ok(new { count });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? jobType,
        [FromQuery] string? employmentType,
        [FromQuery] string? location,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(new GetJobFeedQuery(
            UserId: userId,
            Search: search,
            Category: category,
            JobType: jobType,
            EmploymentType: employmentType,
            Location: location,
            PageSize: pageSize), ct);

        return Ok(result.Items);
    }
}