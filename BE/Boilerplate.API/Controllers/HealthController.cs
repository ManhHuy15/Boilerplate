using Boilerplate.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Boilerplate.API.Controllers;

[Route("api/[controller]")]
public sealed class HealthController(
    HealthCheckService healthCheckService,
    IHostEnvironment environment) : BaseController
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<ApiResponse<HealthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<HealthResponse>>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<HealthResponse>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);
        var response = new HealthResponse(
            report.Status.ToString(),
            environment.EnvironmentName,
            DateTimeOffset.UtcNow);

        return report.Status == HealthStatus.Healthy
            ? Success(response)
            : StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiResponse<HealthResponse>.Fail(
                    "One or more health checks failed.",
                    [new ApiError("unhealthy", "A health check reported an unhealthy status.")],
                    HttpContext.TraceIdentifier,
                    response));
    }
}

public sealed record HealthResponse(
    string Status,
    string Environment,
    DateTimeOffset Timestamp);
