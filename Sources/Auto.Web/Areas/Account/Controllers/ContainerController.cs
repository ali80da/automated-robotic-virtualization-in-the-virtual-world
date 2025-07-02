using Docker.DotNet.Models;
using Docker.DotNet;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Auto.Core.Extensions.StaticExtensions;
using System.Runtime.Versioning;

namespace Auto.Web.Areas.Account.Controllers;

public class ContainerController : SharedAccController
{
    private readonly DockerClient DockerClient = DockerClientFactory.Create();

    /// <summary>
    /// نمایش صفحه اصلی کانتینرها.
    /// </summary>
    /// <returns></returns>
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
            using var stream = await DockerClient.Containers.GetContainerLogsAsync(id, new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Follow = true,
                Tail = "100"
            }, cancellationToken);

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




    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken = default)
    {
        try
        {
            var containers = await DockerClient.Containers.ListContainersAsync(
                new ContainersListParameters { All = true },
                cancellationToken);

            var vm = new DashboardViewModel
            {
                TotalContainers = containers.Count,
                Running = containers.Count(c => c.State == "running"),
                Exited = containers.Count(c => c.State == "exited"),
                ImageUsage = containers
                    .GroupBy(c => c.Image)
                    .ToDictionary(g => g.Key, g => g.Count())
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


}

public class ContainerViewModel
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<string>? Ports { get; set; }
}
public class DashboardViewModel
{
    public int TotalContainers { get; set; }
    public int Running { get; set; }
    public int Exited { get; set; }
    public Dictionary<string, int>? ImageUsage { get; set; }
}
