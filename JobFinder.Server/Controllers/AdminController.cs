using JobFinder.Core.Entities.Common;
using JobFinder.Core.Entities.Identity;
using JobFinder.Infrastructure.Data;
using JobFinder.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[IgnoreAntiforgeryToken]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ═══════════════════════════════════════════════════════════════
    // STATISTICI DASHBOARD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var totalUsers = await _db.Users.CountAsync(ct);
        var totalCandidates = await _db.CandidateProfiles.CountAsync(ct);
        var totalEmployers = await _db.EmployerProfiles.CountAsync(ct);
        var activeJobs = await _db.JobPostings
            .CountAsync(j => j.Status == JobStatus.Published, ct);
        var totalApps = await _db.Applications.CountAsync(ct);
        var pendingVerif = await _db.EmployerProfiles
            .CountAsync(e => !e.IsVerified, ct);

        return Ok(new
        {
            totalUsers,
            totalCandidates,
            totalEmployers,
            activeJobs,
            totalApps,
            pendingVerif,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILIZATORI
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        CancellationToken ct)
    {
        var users = await _db.Users.AsNoTracking().ToListAsync(ct);

        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "—";

            if (!string.IsNullOrEmpty(role) && userRole != role)
                continue;

            if (!string.IsNullOrEmpty(search) &&
                !(user.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) &&
                !(user.UserName ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
                continue;

            result.Add(new
            {
                user.Id,
                user.Email,
                user.UserName,
                Role = userRole,
                UserType = user.UserType,
                IsActive = !user.LockoutEnd.HasValue ||
                              user.LockoutEnd <= DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd,
            });
        }

        return Ok(result);
    }

    [HttpPut("users/{userId}/toggle-lock")]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var isLocked = user.LockoutEnd.HasValue &&
                       user.LockoutEnd > DateTimeOffset.UtcNow;

        if (isLocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            return Ok(new { message = "Contul a fost activat.", isActive = true });
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(
                user, DateTimeOffset.UtcNow.AddYears(100));
            return Ok(new { message = "Contul a fost dezactivat.", isActive = false });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CANDIDAȚI
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = _db.CandidateProfiles.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c =>
                c.FirstName.Contains(search) ||
                c.LastName.Contains(search));

        var list = await query
            .OrderByDescending(c => c.Id)
            .Take(100)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                //c.Title,
                c.Location,
                c.UserId,
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("candidates/{id:int}")]
    public async Task<IActionResult> GetCandidate(int id, CancellationToken ct)
    {
        var candidate = await _db.CandidateProfiles
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Skills).ThenInclude(s => s.Skill)
            .Include(c => c.Certifications)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (candidate == null) return NotFound();

        return Ok(new
        {
            candidate.Id,
            candidate.FirstName,
            candidate.LastName,
            candidate.UserId,
          //  candidate.Title,
            candidate.Location,
           // candidate.Bio,
            candidate.Phone,
            Skills = candidate.Skills.Select(s => s.Skill.Name),
            Experiences = candidate.Experiences.Select(e => new
            {
                e.CompanyName,
                e.Position,
                e.StartDate,
                e.EndDate,
            }),
            Educations = candidate.Educations.Select(e => new
            {
                e.Institution,
                e.Degree,
                e.StartDate,
                e.EndDate,
            }),
        });
    }

    [HttpDelete("candidates/{userId}")]
    public async Task<IActionResult> DeleteCandidate(
        string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (profile != null)
            _db.CandidateProfiles.Remove(profile);

        await _db.SaveChangesAsync(ct);
        await _userManager.DeleteAsync(user);

        return Ok(new { message = "Candidatul a fost șters." });
    }

    // ═══════════════════════════════════════════════════════════════
    // ANGAJATORI
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("employers")]
    public async Task<IActionResult> GetAllEmployers(
        [FromQuery] string? search,
        [FromQuery] bool? verified,
        CancellationToken ct)
    {
        var query = _db.EmployerProfiles.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(e => e.CompanyName.Contains(search));

        if (verified.HasValue)
            query = query.Where(e => e.IsVerified == verified.Value);

        var list = await query
            .OrderByDescending(e => e.Id)
            .Take(100)
            .Select(e => new
            {
                e.Id,
                e.CompanyName,
                e.Industry,
                e.ContactEmail,
                e.IsVerified,
                e.UserId,
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpGet("employers/{id:int}")]
    public async Task<IActionResult> GetEmployer(int id, CancellationToken ct)
    {
        var employer = await _db.EmployerProfiles
            .Include(e => e.Locations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (employer == null) return NotFound();

        var jobCount = await _db.JobPostings
            .CountAsync(j => j.EmployerProfileId == id, ct);

        return Ok(new
        {
            employer.Id,
            employer.CompanyName,
            employer.Industry,
            employer.Website,
            employer.Description,
            employer.FiscalCode,
            employer.ContactEmail,
            employer.IsVerified,
            employer.CompanySize,
            employer.FoundedYear,
            employer.UserId,
            JobCount = jobCount,
            Locations = employer.Locations.Select(l => new { l.City, l.Country }),
        });
    }

    [HttpDelete("employers/{userId}")]
    public async Task<IActionResult> DeleteEmployer(
        string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var profile = await _db.EmployerProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);
        if (profile != null)
            _db.EmployerProfiles.Remove(profile);

        await _db.SaveChangesAsync(ct);
        await _userManager.DeleteAsync(user);

        return Ok(new { message = "Angajatorul a fost șters." });
    }

    // ═══════════════════════════════════════════════════════════════
    // ANUNȚURI
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = _db.JobPostings
            .Include(j => j.EmployerProfile)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(j =>
                j.Title.Contains(search) ||
                j.EmployerProfile.CompanyName.Contains(search));

        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Take(100)
            .Select(j => new
            {
                j.Id,
                j.Title,
                CompanyName = j.EmployerProfile.CompanyName,
                j.Status,
                j.CreatedAt,
                j.PublishedAt,
                j.Location,
            })
            .ToListAsync(ct);

        return Ok(jobs);
    }

    [HttpPut("jobs/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateJob(int id, CancellationToken ct)
    {
        var job = await _db.JobPostings.FindAsync(new object[] { id }, ct);
        if (job == null) return NotFound();

        job.Status = JobStatus.Closed;
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Anunțul a fost dezactivat." });
    }

    [HttpDelete("jobs/{id:int}")]
    public async Task<IActionResult> DeleteJob(int id, CancellationToken ct)
    {
        var job = await _db.JobPostings.FindAsync(new object[] { id }, ct);
        if (job == null) return NotFound();

        _db.JobPostings.Remove(job);
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Anunțul a fost șters." });
    }

    // ═══════════════════════════════════════════════════════════════
    // VERIFICARE ANGAJATORI
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("employer-verifications")]
    public async Task<IActionResult> GetPendingEmployers(CancellationToken ct)
    {
        var employers = await _db.EmployerProfiles
            .Where(e => !e.IsVerified)
            .Include(e => e.Locations)
            .AsNoTracking()
            .OrderByDescending(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.CompanyName,
                e.Industry,
                e.Website,
                e.Description,
                e.FiscalCode,
                e.ContactEmail,
                e.IsVerified,
                e.UserId,
            })
            .ToListAsync(ct);

        return Ok(employers);
    }

    [HttpPut("employer-verifications/{id:int}/approve")]
    public async Task<IActionResult> ApproveEmployer(int id, CancellationToken ct)
    {
        var employer = await _db.EmployerProfiles
            .FindAsync(new object[] { id }, ct);
        if (employer == null) return NotFound();

        employer.IsVerified = true;
        await _db.SaveChangesAsync(ct);

        // Notificare angajator
        _db.Notifications.Add(new Notification
        {
            UserId = employer.UserId,
            Target = NotificationTarget.Employer,
            Type = NotificationType.SystemSuggestion,
            Title = "Cont verificat ✓",
            Message = "Contul tău de angajator a fost verificat și aprobat. Poți publica anunțuri acum.",
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
        });
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Angajatorul a fost verificat." });
    }

    [HttpPut("employer-verifications/{id:int}/reject")]
    public async Task<IActionResult> RejectEmployer(
        int id,
        [FromBody] RejectEmployerDto dto,
        CancellationToken ct)
    {
        var employer = await _db.EmployerProfiles
            .FindAsync(new object[] { id }, ct);
        if (employer == null) return NotFound();

        // Notificare angajator
        _db.Notifications.Add(new Notification
        {
            UserId = employer.UserId,
            Target = NotificationTarget.Employer,
            Type = NotificationType.SystemSuggestion,
            Title = "Cont respins",
            Message = string.IsNullOrWhiteSpace(dto.Reason)
                ? "Contul tău de angajator a fost respins de administrator."
                : $"Contul tău a fost respins. Motiv: {dto.Reason}",
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
        });
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Angajatorul a fost respins." });
    }

    // ═══════════════════════════════════════════════════════════════
    // JURNAL
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? search,
        [FromQuery] string? type,
        CancellationToken ct)
    {
        var query = _db.Notifications.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(n =>
                n.Title.Contains(search) ||
                n.Message.Contains(search));

        if (!string.IsNullOrEmpty(type) &&
            Enum.TryParse<NotificationType>(type, out var nt))
            query = query.Where(n => n.Type == nt);

        var logs = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.Target,
                n.UserId,
                n.CreatedAt,
                n.IsRead,
            })
            .ToListAsync(ct);

        return Ok(logs);
    }
}

// ── DTOs locale ───────────────────────────────────────────────────────
public class RejectEmployerDto
{
    public string? Reason { get; set; }
}