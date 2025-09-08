using Microsoft.AspNetCore.Mvc;

namespace Laraue.Apps.RealEstate.WorkerHost.Controllers;

public class HealthCheckController : ControllerBase
{
    [HttpGet]
    [Route("api/health")]
    public async Task<HealthResponse> GetHealthAsync(CancellationToken ct)
    {
        return new HealthResponse();
    }
}