using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using WaitifyApi.Helpers;

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

    [HttpGet("azure")]
    public async Task<IActionResult> GetAzureBlobStorageService()
    {
        var report = await _service.CheckHealthAsync();

        _logger.LogInformation("Get Health Information : {@0}", JsonResponseHelper.JsonConversion(report));

        return report.Status == HealthStatus.Healthy ? Ok(report) : StatusCode((int)HttpStatusCode.ServiceUnavailable, report);
    }
}
