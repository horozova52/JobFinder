using JobFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class AvatarController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AvatarController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

  
    [HttpPost]
    [RequestSizeLimit(2 * 1024 * 1024)] // 2MB
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Utilizator neidentificat" });

        // Validare fișier
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Niciun fișier selectat" });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "Fișierul depășește limita de 2MB" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Doar JPG, PNG sau WEBP sunt acceptate" });

        // Creăm folderul dacă nu există
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadsFolder);

        // Generăm nume unic bazat pe userId (suprascrie poza veche automat)
        var extension = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"avatar_{userId}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        var avatarUrl = $"/uploads/avatars/{fileName}";

        // Salvăm fișierul pe disc
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Actualizăm AvatarUrl în DB direct (fără CQRS — e o operație simplă de câmp)
        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound(new { message = "Profilul nu a fost găsit" });

        profile.AvatarUrl = avatarUrl;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { avatarUrl });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound();

        // Ștergem fișierul de pe disc
        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            var filePath = Path.Combine(_env.WebRootPath, profile.AvatarUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        profile.AvatarUrl = null;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Fotografia a fost ștearsă" });
    }
}