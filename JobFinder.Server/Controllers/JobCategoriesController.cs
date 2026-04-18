using JobFinder.Infrastructure.Data;
using JobFinder.Shared.DTOs.Jobs;
using JobFinder.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class JobCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public JobCategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _db.JobCategories
            .OrderBy(c => c.Name)
            .Select(c => new JobCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon ?? "",
                JobCount = _db.JobPostings
                    .Count(j => j.CategoryId == c.Id && j.Status == JobStatus.Published)
            })
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }
}
