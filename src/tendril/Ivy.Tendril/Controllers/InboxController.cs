using Ivy.Tendril.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Tendril.Controllers;

[ApiController]
[Route("api/inbox")]
public class InboxController(IJobService jobService, IConfigService configService) : ControllerBase
{
    [HttpPost]
    public IActionResult PostPlan([FromBody] CreatePlanRequest request)
    {
        var apiKey = configService.Settings.Api?.ApiKey;
        if (!string.IsNullOrEmpty(apiKey))
        {
            var providedKey = Request.Headers["X-Api-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(providedKey) || providedKey != apiKey)
                return Unauthorized(new { error = "Invalid or missing API key" });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Description is required" });

        try
        {
            var project = request.Project ?? "Auto";
            var args = new List<string> { "-Description", request.Description, "-Project", project };
            if (!string.IsNullOrEmpty(request.SourcePath))
                args.AddRange(["-SourcePath", request.SourcePath]);

            var jobId = jobService.StartJob("MakePlan", args.ToArray(), null);
            return Ok(new { jobId, status = "Started", message = "Plan creation job started successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to start plan creation: {ex.Message}" });
        }
    }
}

public record CreatePlanRequest(
    string Description,
    string? Project = null,
    string? SourcePath = null
);
