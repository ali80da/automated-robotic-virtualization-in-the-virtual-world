using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Auto.Core.Extensions.StaticExtensions;
using Auto.Core.Services.Docker;

namespace Auto.Web.Areas.Account.Controllers;

public class ContainerController(IDockerService DockerService) : SharedAccController
{
    private readonly IDockerService DockerService = DockerService;

    #region Container List

    /// <summary>
    /// Show a limited list of containers.
    /// </summary>
    [HttpGet("containers")]
    public async Task<IActionResult> Main(CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken = default)
    {
        try
        {
            var containers = await DockerService.ListContainersAsync(all: true, cancellationToken);

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

    [HttpPost("start/{id}")]
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

    [HttpPost("stop/{id}")]
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

    [HttpPost("remove/{id}")]
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
}

#region ViewModels

public class ContainerViewModel
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<string>? Ports { get; set; }
}

public class DashboardViewModel
{
    public int TotalContainers { get; set; }
    public int Running { get; set; }
    public int Exited { get; set; }
    public Dictionary<string, int>? ImageUsage { get; set; }

    public List<ContainerViewModel> Containers { get; set; } = new();
}

#endregion
