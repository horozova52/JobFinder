using JobFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Server.Controllers;


[ApiController]
[Route("api/demo")]
public class DemoSeedController : ControllerBase
{
    private readonly DemoSeeder _demoSeeder;

    public DemoSeedController(DemoSeeder demoSeeder)
    {
        _demoSeeder = demoSeeder;
    }

    [HttpGet("seed")]
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        try
        {
            var result = await _demoSeeder.SeedAsync(ct);
            return Ok(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Eroare la rularea seed-ului demo.",
                detail = ex.Message,
            });
        }
    }
}