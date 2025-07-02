using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Auto.Core.Extensions.StaticExtensions;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Auto.Core.Services.Docker;

public interface IDockerService
{
    #region Container Listing

    /// <summary>
    /// List Docker containers with optional filtering.
    /// </summary>
    Task<IList<ContainerListResponse>> ListContainersAsync(bool all = false, CancellationToken cancellationToken = default);

    #endregion

    #region Container Logs & Inspection

    /// <summary>
    /// Stream container logs as a text stream.
    /// </summary>
    Task<Stream> GetContainerLogsAsync(string containerId, bool follow = true, string tail = "100", CancellationToken cancellationToken = default);

    /// <summary>
    /// Inspect a container for detailed metadata.
    /// </summary>
    Task<ContainerInspectResponse> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get CPU and memory stats for a running container.
    /// </summary>
    Task<(string cpu, string mem)> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken = default);

    #endregion

    #region Container Lifecycle Management

    /// <summary>
    /// Start a stopped container.
    /// </summary>
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a running container.
    /// </summary>
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restart a container.
    /// </summary>
    Task RestartContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a container from the host.
    /// </summary>
    Task RemoveContainerAsync(string containerId, bool force = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all stopped containers.
    /// </summary>
    Task PruneContainersAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Advanced Operations

    /// <summary>
    /// Export a container's filesystem as a .tar archive and return the path.
    /// </summary>
    Task<string> ExportContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create and start a new container from an existing image.
    /// </summary>
    Task RecreateContainerFromImageAsync(string image, CancellationToken cancellationToken = default);

    #endregion



    #region Image Management

    /// <summary>
    /// Get a list of local Docker images.
    /// </summary>
    Task<IList<ImagesListResponse>> ListImagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pull a Docker image from registry.
    /// </summary>
    Task PullImageAsync(string imageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a Docker image by ID or name.
    /// </summary>
    Task RemoveImageAsync(string imageIdOrName, bool force = false, CancellationToken cancellationToken = default);

    #endregion


    #region Volume Management

    /// <summary>
    /// Retrieves a list of all Docker volumes.
    /// </summary>
    Task<IList<VolumeResponse>> ListVolumesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific Docker volume by name.
    /// </summary>
    Task RemoveVolumeAsync(string volumeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all unused Docker volumes (dangling).
    /// </summary>
    Task PruneVolumesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Network Management

    /// <summary>
    /// Retrieves a list of all Docker networks.
    /// </summary>
    Task<IList<NetworkResponse>> ListNetworksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new Docker network with the specified name and driver.
    /// </summary>
    Task CreateNetworkAsync(string name, string driver = "bridge", CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a Docker network by ID.
    /// </summary>
    Task RemoveNetworkAsync(string networkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects a container to a Docker network.
    /// </summary>
    Task ConnectContainerToNetworkAsync(string containerId, string networkId, CancellationToken cancellationToken = default);

    #endregion
}


public class DockerService : IDockerService
{
    private readonly DockerClient DockerClient = DockerClientFactory.Create();

    #region Container Listing

    public async Task<IList<ContainerListResponse>> ListContainersAsync(bool all = false, CancellationToken cancellationToken = default)
    {
        return await DockerClient.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = all
        }, cancellationToken);
    }

    #endregion

    #region Container Logs & Inspection

    public async Task<Stream> GetContainerLogsAsync(string containerId, bool follow = true, string tail = "100", CancellationToken cancellationToken = default)
    {
        return await DockerClient.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Follow = follow,
            Tail = tail
        }, cancellationToken);
    }

    public async Task<ContainerInspectResponse> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        return await DockerClient.Containers.InspectContainerAsync(containerId, cancellationToken);
    }

    //[Obsolete]
    public async Task<(string cpu, string mem)> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken = default)
    {
        using var statsStream = await DockerClient.Containers.GetContainerStatsAsync(containerId,
        new ContainerStatsParameters { Stream = true },
        cancellationToken);

        using var reader = new StreamReader(statsStream);
        var jsonLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrEmpty(jsonLine))
            return ("N/A", "N/A");

        var doc = JsonDocument.Parse(jsonLine);

        var cpu = doc.RootElement.TryGetProperty("cpu_stats", out var cpuStats) &&
                  cpuStats.TryGetProperty("cpu_usage", out var usage) &&
                  usage.TryGetProperty("total_usage", out var total)
            ? $"{total.GetInt64() / 1_000_000} ms"
            : "N/A";

        var mem = doc.RootElement.TryGetProperty("memory_stats", out var memStats) &&
                  memStats.TryGetProperty("usage", out var memUsage)
            ? $"{(memUsage.GetInt64() / 1024.0 / 1024.0):F1} MB"
            : "N/A";

        return (cpu, mem);
    }

    #endregion

    #region Container Lifecycle Management

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var success = await DockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters(), cancellationToken);
        if (!success)
            throw new InvalidOperationException($"Failed to start container: {containerId}");
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await DockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), cancellationToken);
    }

    public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await DockerClient.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters(), cancellationToken);
    }

    public async Task RemoveContainerAsync(string containerId, bool force = false, CancellationToken cancellationToken = default)
    {
        await DockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
        {
            Force = force
        }, cancellationToken);
    }

    public async Task PruneContainersAsync(CancellationToken cancellationToken = default)
    {
        await DockerClient.Containers.PruneContainersAsync(new ContainersPruneParameters(), cancellationToken);
    }

    #endregion

    #region Advanced Operations

    public async Task<string> ExportContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        using var stream = await DockerClient.Containers.ExportContainerAsync(containerId, cancellationToken);
        var exportPath = Path.Combine(Path.GetTempPath(), $"{containerId}.tar");

        using var file = File.Create(exportPath);
        await stream.CopyToAsync(file, cancellationToken);

        return exportPath;
    }

    public async Task RecreateContainerFromImageAsync(string image, CancellationToken cancellationToken = default)
    {
        var createParams = new CreateContainerParameters
        {
            Image = image,
            Name = $"recreated_{Guid.NewGuid():N}".Substring(0, 12),
            Tty = true
        };

        var created = await DockerClient.Containers.CreateContainerAsync(createParams, cancellationToken);
        await DockerClient.Containers.StartContainerAsync(created.ID, new ContainerStartParameters(), cancellationToken);
    }

    #endregion





    #region Image Management

    public async Task<IList<ImagesListResponse>> ListImagesAsync(CancellationToken cancellationToken = default)
    {
        return await DockerClient.Images.ListImagesAsync(new ImagesListParameters { All = true }, cancellationToken);
    }

    public async Task PullImageAsync(string imageName, CancellationToken cancellationToken = default)
    {
        var parts = imageName.Split(':');
        var repo = parts[0];
        var tag = parts.Length > 1 ? parts[1] : "latest";

        await DockerClient.Images.CreateImageAsync(
            new ImagesCreateParameters { FromImage = repo, Tag = tag },
            new AuthConfig(), // بدون احراز هویت برای public registry
            new Progress<JSONMessage>(),
            cancellationToken
        );
    }

    public async Task RemoveImageAsync(string imageIdOrName, bool force = false, CancellationToken cancellationToken = default)
    {
        await DockerClient.Images.DeleteImageAsync(
            imageIdOrName,
            new ImageDeleteParameters { Force = force },
            cancellationToken
        );
    }

    #endregion




    #region Volume Management

    public async Task<IList<VolumeResponse>> ListVolumesAsync(CancellationToken cancellationToken = default)
    {
        var result = await DockerClient.Volumes.ListAsync(new VolumesListParameters(), cancellationToken);
        return result.Volumes;
    }

    public async Task RemoveVolumeAsync(string volumeName, CancellationToken cancellationToken = default)
    {
        await DockerClient.Volumes.RemoveAsync(volumeName, force: true, cancellationToken);
    }

    public async Task PruneVolumesAsync(CancellationToken cancellationToken = default)
    {
        await DockerClient.Volumes.PruneAsync(new VolumesPruneParameters(), cancellationToken);
    }

    #endregion

    #region Network Management

    public async Task<IList<NetworkResponse>> ListNetworksAsync(CancellationToken cancellationToken = default)
    {
        return await DockerClient.Networks.ListNetworksAsync(new NetworksListParameters(), cancellationToken);
    }

    public async Task CreateNetworkAsync(string name, string driver = "bridge", CancellationToken cancellationToken = default)
    {
        var createParams = new NetworksCreateParameters
        {
            Name = name,
            Driver = driver
        };

        await DockerClient.Networks.CreateNetworkAsync(createParams, cancellationToken);
    }

    public async Task RemoveNetworkAsync(string networkId, CancellationToken cancellationToken = default)
    {
        await DockerClient.Networks.DeleteNetworkAsync(networkId, cancellationToken);
    }

    public async Task ConnectContainerToNetworkAsync(string containerId, string networkId, CancellationToken cancellationToken = default)
    {
        var endpointSettings = new EndpointSettings();
        await DockerClient.Networks.ConnectNetworkAsync(networkId, new NetworkConnectParameters
        {
            Container = containerId,
            EndpointConfig = endpointSettings
        }, cancellationToken);
    }

    #endregion



}