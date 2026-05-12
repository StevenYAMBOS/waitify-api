using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace WaitifyApi.Controllers;

[Controller]
[Route("health")]
public class HealthController(ILogger<HealthController> logger, HealthCheckService service) : ControllerBase
{
    private readonly ILogger<HealthController> _logger = logger;
    private readonly HealthCheckService _service = service;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var report = await _service.CheckHealthAsync();

        _logger.LogInformation("Get Health Information : {@0}", report);

        return report.Status == HealthStatus.Healthy ? Ok(report) : StatusCode((int)HttpStatusCode.ServiceUnavailable, report);
    }
}
