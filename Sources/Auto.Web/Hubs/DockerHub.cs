using Auto.Core.Extensions.StaticExtensions;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;

namespace Auto.Web.Hubs;

public class DockerHub : Hub
{
    private static readonly ConcurrentDictionary<string, MultiplexedStream> TerminalSessions = new();
    private readonly DockerClient _docker = DockerClientFactory.Create();

    /// <summary>
    /// اتصال به ترمینال کانتینر از طریق exec
    /// </summary>
    public async Task JoinTerminal(string containerId)
    {
        try
        {
            var exec = await _docker.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
            {
                AttachStdin = true,
                AttachStdout = true,
                AttachStderr = true,
                Tty = true,
                Cmd = new[] { "sh", "-c", "exec bash || exec sh" } // bash یا sh
            });

            var stream = await _docker.Exec.StartAndAttachContainerExecAsync(exec.ID, false);
            if (stream == null)
            {
                await Clients.Caller.SendAsync("TerminalError", "Stream not available.");
                return;
            }

            TerminalSessions[Context.ConnectionId] = stream;

            _ = Task.Run(async () =>
            {
                var buffer = new byte[1024];

                try
                {
                    while (!Context.ConnectionAborted.IsCancellationRequested)
                    {
                        var result = await stream.ReadOutputAsync(buffer, 0, buffer.Length, Context.ConnectionAborted);
                        if (result.EOF)
                            break;

                        var output = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await Clients.Caller.SendAsync("ReceiveTerminalOutput", output);
                    }
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("TerminalError", $"Terminal stream error: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("TerminalError", $"Terminal initialization failed: {ex.Message}");
        }
    }


    /// <summary>
    /// ارسال ورودی به ترمینال
    /// </summary>
    public async Task SendInputToTerminal(string input)
    {
        if (TerminalSessions.TryGetValue(Context.ConnectionId, out var stream))
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            await stream.WriteAsync(buffer, 0, buffer.Length, Context.ConnectionAborted);
        }
        else
        {
            await Clients.Caller.SendAsync("TerminalError", "No active terminal session.");
        }
    }


    /// <summary>
    /// استریم زنده آمار منابع (CPU/RAM) برای کانتینر
    /// </summary>
    public async Task StreamStats(string containerId)
    {
        try
        {
            var stats = await _docker.Containers.GetContainerStatsAsync(
                containerId,
                new ContainerStatsParameters { Stream = true },
                Context.ConnectionAborted
            );

            using var reader = new StreamReader(stats);
            while (!Context.ConnectionAborted.IsCancellationRequested && !reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    await Clients.Caller.SendAsync("ReceiveContainerStats", line);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("StatsError", $"Stats stream error: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TerminalSessions.TryRemove(Context.ConnectionId, out var stream))
        {
            stream.Dispose();
        }

        await base.OnDisconnectedAsync(exception);
    }





















}