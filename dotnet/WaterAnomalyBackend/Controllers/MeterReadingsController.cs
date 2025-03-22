using Microsoft.AspNetCore.Mvc;
using WaterAnomalyBackend.Services;

namespace WaterAnomalyBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeterReadingsController : ControllerBase
{
    private readonly IMeterReadingRepository _repo;

    public MeterReadingsController(IMeterReadingRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("{meterId}")]
    public async Task<IActionResult> GetRecent(string meterId, [FromQuery] int hours = 168)
    {
        var readings = await _repo.GetRecentAsync(meterId, hours);
        return Ok(readings);
    }
}
