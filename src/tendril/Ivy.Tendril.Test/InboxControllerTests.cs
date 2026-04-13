using Ivy.Tendril.Controllers;
using Ivy.Tendril.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Ivy.Tendril.Test;

public class InboxControllerTests
{
    [Fact]
    public void PostPlan_ValidRequest_ReturnsOkWithJobId()
    {
        var jobService = new StubJobService();
        var configService = new ConfigService(new TendrilSettings(), "/tmp");
        var controller = CreateController(jobService, configService);

        var result = controller.PostPlan(new CreatePlanRequest("Fix a bug", "Tendril"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Single(jobService.StartedJobs);
        Assert.Equal("MakePlan", jobService.StartedJobs[0].Type);
    }

    [Fact]
    public void PostPlan_EmptyDescription_ReturnsBadRequest()
    {
        var jobService = new StubJobService();
        var configService = new ConfigService(new TendrilSettings(), "/tmp");
        var controller = CreateController(jobService, configService);

        var result = controller.PostPlan(new CreatePlanRequest(""));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(jobService.StartedJobs);
    }

    [Fact]
    public void PostPlan_WithAuthentication_ValidKey_ReturnsOk()
    {
        var jobService = new StubJobService();
        var settings = new TendrilSettings { Api = new ApiSettings { ApiKey = "secret-123" } };
        var configService = new ConfigService(settings, "/tmp");
        var controller = CreateController(jobService, configService, apiKey: "secret-123");

        var result = controller.PostPlan(new CreatePlanRequest("Add feature"));

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(jobService.StartedJobs);
    }

    [Fact]
    public void PostPlan_WithAuthentication_InvalidKey_ReturnsUnauthorized()
    {
        var jobService = new StubJobService();
        var settings = new TendrilSettings { Api = new ApiSettings { ApiKey = "secret-123" } };
        var configService = new ConfigService(settings, "/tmp");
        var controller = CreateController(jobService, configService, apiKey: "wrong-key");

        var result = controller.PostPlan(new CreatePlanRequest("Add feature"));

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Empty(jobService.StartedJobs);
    }

    [Fact]
    public void PostPlan_WithAuthentication_MissingKey_ReturnsUnauthorized()
    {
        var jobService = new StubJobService();
        var settings = new TendrilSettings { Api = new ApiSettings { ApiKey = "secret-123" } };
        var configService = new ConfigService(settings, "/tmp");
        var controller = CreateController(jobService, configService);

        var result = controller.PostPlan(new CreatePlanRequest("Add feature"));

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Empty(jobService.StartedJobs);
    }

    [Fact]
    public void PostPlan_NoAuthConfigured_AllowsAccess()
    {
        var jobService = new StubJobService();
        var configService = new ConfigService(new TendrilSettings(), "/tmp");
        var controller = CreateController(jobService, configService);

        var result = controller.PostPlan(new CreatePlanRequest("Do something"));

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(jobService.StartedJobs);
    }

    [Fact]
    public void PostPlan_WithSourcePath_PassesItToJobService()
    {
        var jobService = new StubJobService();
        var configService = new ConfigService(new TendrilSettings(), "/tmp");
        var controller = CreateController(jobService, configService);

        var result = controller.PostPlan(new CreatePlanRequest("Fix bug", "Tendril", @"D:\Tests\Session1"));

        Assert.IsType<OkObjectResult>(result);
        var job = Assert.Single(jobService.StartedJobs);
        Assert.Contains("-SourcePath", job.Args);
        Assert.Contains(@"D:\Tests\Session1", job.Args);
    }

    [Fact]
    public void PostPlan_NullProject_DefaultsToAuto()
    {
        var jobService = new StubJobService();
        var configService = new ConfigService(new TendrilSettings(), "/tmp");
        var controller = CreateController(jobService, configService);

        var result = controller.PostPlan(new CreatePlanRequest("Some task"));

        Assert.IsType<OkObjectResult>(result);
        var job = Assert.Single(jobService.StartedJobs);
        Assert.Contains("Auto", job.Args);
    }

    private static InboxController CreateController(
        IJobService jobService,
        IConfigService configService,
        string? apiKey = null)
    {
        var controller = new InboxController(jobService, configService);
        var httpContext = new DefaultHttpContext();
        if (apiKey != null)
            httpContext.Request.Headers["X-Api-Key"] = apiKey;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private class StubJobService : IJobService
    {
        public List<(string Type, string[] Args)> StartedJobs { get; } = new();

        public string StartJob(string type, string[] args, string? inboxFilePath)
        {
            var id = $"job-{StartedJobs.Count + 1}";
            StartedJobs.Add((type, args));
            return id;
        }

        public string StartJob(string type, params string[] args)
        {
            return StartJob(type, args, null);
        }

        public void CompleteJob(string id, int? exitCode, bool timedOut = false, bool staleOutput = false) { }
        public void StopJob(string id) { }
        public void DeleteJob(string id) { }
        public void ClearCompletedJobs() { }
        public void ClearFailedJobs() { }
        public List<Ivy.Tendril.Apps.Jobs.JobItem> GetJobs() => new();
        public Ivy.Tendril.Apps.Jobs.JobItem? GetJob(string id) => null;
        public bool IsInboxFileTracked(string filePath) => false;
        public void Dispose() { }

#pragma warning disable CS0067
        public event Action? JobsChanged;
        public event Action<JobNotification>? NotificationReady;
#pragma warning restore CS0067
    }
}
