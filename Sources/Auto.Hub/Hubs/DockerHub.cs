using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Docker.DotNet;
using Microsoft.AspNetCore.SignalR;

namespace Auto.Hub.Hubs;

public class DockerHub : Hub
{
    private static readonly ConcurrentDictionary<string, MultiplexedStream> TerminalSessions = new();
    private readonly DockerClient _docker = DockerClientFactory.Create();

    // اتصال به ترمینال
    public async Task JoinTerminal(string containerId)
    {
        var exec = await _docker.Containers.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
        {
            AttachStdin = true,
            AttachStdout = true,
            AttachStderr = true,
            Tty = true,
            Cmd = new[] { "/bin/bash" }
        });

        var stream = await _docker.Containers.StartAndAttachContainerExecAsync(exec.ID, false);
        TerminalSessions[Context.ConnectionId] = stream;

        _ = Task.Run(async () =>
        {
            using var reader = new StreamReader(stream);
            char[] buffer = new char[1024];
            int bytesRead;

            while ((bytesRead = await reader.ReadAsync(buffer)) > 0)
            {
                await Clients.Caller.SendAsync("ReceiveTerminalOutput", new string(buffer, 0, bytesRead));
            }
        });
    }

    // ارسال ورودی به ترمینال
    public async Task SendInputToTerminal(string input)
    {
        if (TerminalSessions.TryGetValue(Context.ConnectionId, out var stream))
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    // استریم آمار منابع (CPU/RAM)
    public async Task StreamStats(string containerId)
    {
        var stats = await _docker.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters { Stream = true });

        using var reader = new StreamReader(stats);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                await Clients.Caller.SendAsync("ReceiveContainerStats", line);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TerminalSessions.TryRemove(Context.ConnectionId, out var stream))
        {
            await stream.DisposeAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }
}