using JobFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class SkillsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SkillsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returnează toate skill-urile disponibile în platformă.
    /// Folosit pentru autocomplete în profilul candidatului și la crearea joburilor.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var skills = await _db.Skills
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Category })
            .ToListAsync(cancellationToken);

        return Ok(skills);
    }
}