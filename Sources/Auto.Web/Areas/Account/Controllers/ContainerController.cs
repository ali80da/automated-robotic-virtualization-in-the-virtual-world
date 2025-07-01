using Docker.DotNet.Models;
using Docker.DotNet;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Auto.Core.Extensions.StaticExtensions;

namespace Auto.Web.Areas.Account.Controllers;

public class ContainerController : SharedAccController
{
    private readonly DockerClient DockerClient = DockerClientFactory.Create();

    /// <summary>
    /// نمایش صفحه اصلی کانتینرها.
    /// </summary>
    /// <returns></returns>
    [HttpGet("containers")]
    public async Task<IActionResult> Main()
    {
        var containers = await DockerClient.Containers.ListContainersAsync(
            new ContainersListParameters()
            {
                Limit = 10
            });

        var result = containers.Select(c => new ContainerViewModel
        {
            ID = c.ID,
            Name = c.Names.FirstOrDefault()!,
            Image = c.Image,
            Ports = [.. c.Ports.Select(p => $"{p.IP}:{p.PublicPort} -> {p.PrivatePort}")]
        });

        return View(result);
    }

    [HttpGet("/stream-monitor/{id}")]
    public async Task<IActionResult> StreamMonitor(string id)
    {
        var stream = await DockerClient.Containers.GetContainerLogsAsync(id, new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Follow = true,
            Tail = "100"
        });

        Response.Headers.Add("Content-Type", "text/event-stream");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                await Response.WriteAsync($"data: {line}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        return new EmptyResult();
    }


    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashborad()
    {
        var containers = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });

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

}

public class ContainerViewModel
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public List<string> Ports { get; set; }
}
public class DashboardViewModel
{
    public int TotalContainers { get; set; }
    public int Running { get; set; }
    public int Exited { get; set; }
    public Dictionary<string, int> ImageUsage { get; set; }
}
