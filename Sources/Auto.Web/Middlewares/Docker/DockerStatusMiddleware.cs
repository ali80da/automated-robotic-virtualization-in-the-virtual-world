using Auto.Core.Services.Docker;

namespace Auto.Web.Middlewares.Docker;

public class DockerStatusMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IDockerService dockerService)
    {
        bool isInstalled = dockerService.IsDockerInstalled();
        bool isRunning = isInstalled && await dockerService.IsDockerRunningAsync();

        // در TempData ذخیره می‌کنیم تا در Layout یا View استفاده شود
        context.Items["DockerStatus"] = new
        {
            IsInstalled = isInstalled,
            IsRunning = isRunning
        };

        await _next(context);
    }
}