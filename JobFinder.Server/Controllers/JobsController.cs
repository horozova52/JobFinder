using AutoMapper;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Jobs;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Jobs.Queries.GetAll;
using JobFinder.UseCases.Features.Jobs.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employer")]
[IgnoreAntiforgeryToken]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public JobsController(IMediator mediator, ApplicationDbContext db, IMapper mapper)
    {
        _mediator = mediator;
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllJobPostingsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetJobPostingByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobPostingDto dto, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var profile = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var job = new JobPosting
        {
            EmployerProfileId = profile.Id,
            Title = dto.Title,
            Description = dto.Description,
            Requirements = dto.Requirements,
            Responsibilities = dto.Responsibilities,
            Location = dto.Location,
            JobType = dto.JobType,
            EmploymentType = dto.EmploymentType,
            SalaryFrom = dto.SalaryFrom,
            SalaryTo = dto.SalaryTo,
            IsSalaryNegotiable = dto.IsSalaryNegotiable,
            Status = JobStatus.Published,
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow
        };

        _db.JobPostings.Add(job);
        await _db.SaveChangesAsync(ct);

        var result = _mapper.Map<JobPostingDto>(job);
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateJobPostingDto dto, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var profile = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var job = await _db.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == profile.Id, ct);

        if (job == null)
            return NotFound(new { message = "Anunțul nu a fost găsit" });

        job.Title = dto.Title;
        job.Description = dto.Description;
        job.Requirements = dto.Requirements;
        job.Responsibilities = dto.Responsibilities;
        job.Location = dto.Location;
        job.JobType = dto.JobType;
        job.EmploymentType = dto.EmploymentType;
        job.SalaryFrom = dto.SalaryFrom;
        job.SalaryTo = dto.SalaryTo;
        job.IsSalaryNegotiable = dto.IsSalaryNegotiable;

        await _db.SaveChangesAsync(ct);

        var result = _mapper.Map<JobPostingDto>(job);
        return Ok(result);
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var profile = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var job = await _db.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == profile.Id, ct);

        if (job == null)
            return NotFound(new { message = "Anunțul nu a fost găsit" });

        job.Status = JobStatus.Closed;
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var profile = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
            return BadRequest(new { message = "Profilul de angajator nu a fost găsit" });

        var job = await _db.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id && j.EmployerProfileId == profile.Id, ct);

        if (job == null)
            return NotFound(new { message = "Anunțul nu a fost găsit" });

        _db.JobPostings.Remove(job);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
