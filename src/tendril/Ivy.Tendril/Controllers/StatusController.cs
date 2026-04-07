using Ivy.Tendril.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Tendril.Controllers;

[ApiController]
[Route("api/jobs")]
public class StatusController : ControllerBase
{
    private readonly IJobService _jobService;

    public StatusController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost("{jobId}/status")]
    public IActionResult PostStatus(string jobId, [FromBody] StatusRequest request)
    {
        var job = _jobService.GetJob(jobId);
        if (job == null) return NotFound();
        job.StatusMessage = request.Message;
        return Ok();
    }
}

public record StatusRequest(string Message);