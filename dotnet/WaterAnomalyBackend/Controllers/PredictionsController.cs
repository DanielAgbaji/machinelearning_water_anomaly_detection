using Microsoft.AspNetCore.Mvc;
using WaterAnomalyBackend.Services;

namespace WaterAnomalyBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionsController : ControllerBase
{
    private readonly AnomalyDetectionService _service;
    private readonly IPredictionRepository _repo;

    public PredictionsController(AnomalyDetectionService service, IPredictionRepository repo)
    {
        _service = service;
        _repo = repo;
    }

    [HttpPost("score")]
    public async Task<IActionResult> Score([FromBody] ScoreRequest request, CancellationToken ct)
    {
        var prediction = await _service.ScoreReadingAsync(request, ct);
        return Ok(prediction);
    }

    [HttpGet("meter/{meterId}")]
    public async Task<IActionResult> GetByMeter(string meterId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var predictions = await _repo.GetByMeterAsync(meterId, page, pageSize);
        return Ok(predictions);
    }

    [HttpGet("anomalies")]
    public async Task<IActionResult> GetAnomalies([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var anomalies = await _repo.GetAnomaliesAsync(page, pageSize);
        return Ok(anomalies);
    }
}
