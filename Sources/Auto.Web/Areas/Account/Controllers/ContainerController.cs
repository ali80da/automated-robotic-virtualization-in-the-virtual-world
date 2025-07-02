using Microsoft.AspNetCore.Mvc;
using System.Text;
using Auto.Core.Services.Docker;
using Auto.Core.DataMo.Docker;

namespace Auto.Web.Areas.Account.Controllers;

public class ContainerController(IDockerService DockerService) : SharedAccController
{
    private readonly IDockerService DockerService = DockerService;

    #region Container List

    /// <summary>
    /// Show a limited list of containers.
    /// </summary>
    [HttpGet("containers")]
    public async Task<IActionResult> Containers(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return StatusCode(StatusCodes.Status503ServiceUnavailable);

        try
        {
            var containers = await DockerService.ListContainersAsync(all: false, cancellationToken);
            var result = containers
                .Take(10)
                .Select(c => new ContainerViewModel
                {
                    ID = c.ID,
                    Name = c.Names.FirstOrDefault() ?? "Unnamed",
                    Image = c.Image,
                    Ports = c.Ports.Select(p => $"{p.IP}:{p.PublicPort} -> {p.PrivatePort}").ToList()
                }).ToList();

            return View(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"خطا در دریافت کانتینرها: {ex.Message}");
        }
    }

    #endregion

    #region Stream Logs

    /// <summary>
    /// Stream live container logs.
    /// </summary>
    [HttpGet("stream-monitor/{id}")]
    public async Task<IActionResult> StreamMonitor(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("شناسه کانتینر نامعتبر است.");

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Content-Type-Options", "nosniff");

        try
        {
            using var stream = await DockerService.GetContainerLogsAsync(id, follow: true, tail: "100", cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    await Response.WriteAsync($"data: {line}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }

            return new EmptyResult();
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest, "⛔ درخواست توسط کلاینت لغو شد.");
        }
        catch (Docker.DotNet.DockerApiException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"❌ خطای Docker: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"❗ خطای غیرمنتظره: {ex.Message}");
        }
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// Show container statistics.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(string? search, string? status, string? image, CancellationToken cancellationToken = default)
    {
        try
        {
            var containers = await DockerService.ListContainersAsync(all: true, cancellationToken);

            // Filtering
            if (!string.IsNullOrWhiteSpace(search))
                containers = containers.Where(c =>
                    (c.Names.Any(n => n.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                     c.Image.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();

            if (!string.IsNullOrWhiteSpace(status))
                containers = containers.Where(c => c.State.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(image))
                containers = containers.Where(c => c.Image == image).ToList();

            var vm = new DashboardViewModel
            {
                TotalContainers = containers.Count,
                Running = containers.Count(c => c.State == "running"),
                Exited = containers.Count(c => c.State == "exited"),
                ImageUsage = containers
                            .GroupBy(c => c.Image)
                            .ToDictionary(g => g.Key, g => g.Count()),
                Containers = [.. containers.Select(c => new ContainerViewModel
                {
                    ID = c.ID,
                    Image = c.Image,
                    Status = c.State
                })]
            };

            return View(vm);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "درخواست لغو شد.");
        }
        catch (Docker.DotNet.DockerApiException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"خطای Docker: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"خطای ناشناخته: {ex.Message}");
        }
    }

    #endregion




    #region Container Actions

    #region Start Container

    [HttpPost("start-container/{id}")]
    public async Task<IActionResult> Start(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Invalid container ID");

        try
        {
            await DockerService.StartContainerAsync(id, cancellationToken);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error starting container: {ex.Message}");
        }
    }

    #endregion

    #region Stop Container

    [HttpPost("stop-container/{id}")]
    public async Task<IActionResult> Stop(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Invalid container ID");

        try
        {
            await DockerService.StopContainerAsync(id, cancellationToken);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error stopping container: {ex.Message}");
        }
    }

    #endregion

    #region Remove Container

    [HttpPost("remove-container/{id}")]
    public async Task<IActionResult> Remove(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Invalid container ID");

        try
        {
            await DockerService.RemoveContainerAsync(id, force: true, cancellationToken);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error removing container: {ex.Message}");
        }
    }

    #endregion

    #endregion

    #region Restart Container

    /// <summary>
    /// Restarts a running or stopped container.
    /// </summary>
    /// <param name="id">Container ID</param>
    [HttpPost("restart-container/{id}")]
    public async Task<IActionResult> Restart(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Invalid container ID.");

        try
        {
            await DockerService.RestartContainerAsync(id, ct);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to restart container: {ex.Message}");
        }
    }

    #endregion

    #region Recreate Container from Image

    /// <summary>
    /// Creates and starts a new container from the specified image.
    /// </summary>
    /// <param name="image">Docker image name (e.g., nginx:latest)</param>
    [HttpPost("recreate-container")]
    public async Task<IActionResult> RecreateFromImage(string image, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(image))
            return BadRequest("Image name is required.");

        try
        {
            await DockerService.RecreateContainerFromImageAsync(image, ct);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to recreate container: {ex.Message}");
        }
    }

    #endregion

    #region Prune Containers

    /// <summary>
    /// Deletes all stopped containers from the Docker host.
    /// </summary>
    [HttpPost("prune-container")]
    public async Task<IActionResult> Prune(CancellationToken ct)
    {
        try
        {
            await DockerService.PruneContainersAsync(ct);
            return RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to prune containers: {ex.Message}");
        }
    }

    #endregion
}
