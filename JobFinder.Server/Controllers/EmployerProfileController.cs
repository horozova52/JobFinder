using AutoMapper;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Employers;
using JobFinder.Shared.DTOs.Jobs;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Features.Employers.Commands.UpdateProfile;
using JobFinder.UseCases.Features.Employers.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/employer/profile")]
[Authorize(Roles = "Employer")]
[IgnoreAntiforgeryToken]
public class EmployerProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IMapper _mapper;

    public EmployerProfileController(
        IMediator mediator,
        ApplicationDbContext db,
        IWebHostEnvironment env,
        IMapper mapper)
    {
        _mediator = mediator;
        _db = db;
        _env = env;
        _mapper = mapper;
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

    [HttpPost("logo")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Niciun fișier selectat" });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "Fișierul depășește limita de 2MB" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Doar JPG, PNG sau WEBP sunt acceptate" });

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "logos");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"logo_{userId}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        var logoUrl = $"/uploads/logos/{fileName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var profile = await _db.EmployerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
            return NotFound(new { message = "Profilul nu a fost găsit" });

        profile.LogoUrl = logoUrl;
        await _db.SaveChangesAsync(ct);

        return Ok(new { logoUrl });
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetMyJobs(CancellationToken ct)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        var profile = await _db.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
            return Ok(new List<JobPostingDto>());

        var jobs = await _db.JobPostings
            .Include(j => j.Skills).ThenInclude(s => s.Skill)
            .Where(j => j.EmployerProfileId == profile.Id)
            .AsNoTracking()
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(ct);

        var jobDtos = _mapper.Map<List<JobPostingDto>>(jobs);
        return Ok(jobDtos);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(int id, CancellationToken ct)
    {
        var profile = await _db.EmployerProfiles
            .Include(e => e.Locations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (profile == null)
            return NotFound(new { message = "Compania nu a fost găsită" });

        var profileDto = _mapper.Map<EmployerProfileDto>(profile);

        var jobs = await _db.JobPostings
            .Include(j => j.Skills).ThenInclude(s => s.Skill)
            .Where(j => j.EmployerProfileId == id && j.Status == JobStatus.Published)
            .AsNoTracking()
            .OrderByDescending(j => j.PublishedAt)
            .ToListAsync(ct);

        var jobDtos = _mapper.Map<List<JobPostingDto>>(jobs);

        return Ok(new { profile = profileDto, jobs = jobDtos });
    }

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
