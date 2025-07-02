using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auto.Core.Extensions.StaticExtensions;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Auto.Core.Services.Docker;

public interface IDockerService
{
    #region Container Listing

    /// <summary>
    /// Lists Docker containers.
    /// </summary>
    /// <param name="all">Include stopped containers if true.</param>
    Task<IList<ContainerListResponse>> ListContainersAsync(bool all = false, CancellationToken cancellationToken = default);

    #endregion

    #region Container Logs

    /// <summary>
    /// Streams logs from a container.
    /// </summary>
    Task<Stream> GetContainerLogsAsync(string containerId, bool follow = true, string tail = "100", CancellationToken cancellationToken = default);

    #endregion

    #region Container Inspection

    /// <summary>
    /// Retrieves detailed information about a container.
    /// </summary>
    Task<ContainerInspectResponse> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default);

    #endregion

    #region Container Lifecycle Management

    /// <summary>
    /// Starts a stopped container.
    /// </summary>
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a running container.
    /// </summary>
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a container from the Docker host.
    /// </summary>
    Task RemoveContainerAsync(string containerId, bool force = false, CancellationToken cancellationToken = default);

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

    #region Container Logs

    public async Task<Stream> GetContainerLogsAsync(string containerId, bool follow = true, string tail = "100", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerId))
            throw new ArgumentException("Container ID is required.", nameof(containerId));

        return await DockerClient.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Follow = follow,
            Tail = tail
        }, cancellationToken);
    }

    #endregion

    #region Container Inspection

    public async Task<ContainerInspectResponse> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        return await DockerClient.Containers.InspectContainerAsync(containerId, cancellationToken);
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

    public async Task RemoveContainerAsync(string containerId, bool force = false, CancellationToken cancellationToken = default)
    {
        await DockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
        {
            Force = force
        }, cancellationToken);
    }

    #endregion
}