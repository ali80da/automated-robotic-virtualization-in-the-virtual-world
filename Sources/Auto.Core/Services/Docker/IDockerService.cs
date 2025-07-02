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
    /// <summary>
    /// List containers on the Docker host.
    /// </summary>
    /// <param name="all">Whether to include stopped containers.</param>
    Task<IList<ContainerListResponse>> ListContainersAsync(bool all = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the logs of a container.
    /// </summary>
    /// <param name="containerId">The ID of the container.</param>
    /// <param name="follow">Whether to stream logs.</param>
    /// <param name="tail">Number of lines to show from the end of the logs.</param>
    Task<Stream> GetContainerLogsAsync(string containerId, bool follow = true, string tail = "100", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed information about a specific container.
    /// </summary>
    Task<ContainerInspectResponse> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default);
}


public class DockerService : IDockerService
{
    private readonly DockerClient DockerClient = DockerClientFactory.Create();

    #region List Containers

    /// <summary>
    /// Retrieve a list of containers running (or stopped) on the host.
    /// </summary>
    public async Task<IList<ContainerListResponse>> ListContainersAsync(bool all = false, CancellationToken cancellationToken = default)
    {
        return await DockerClient.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = all
        }, cancellationToken);
    }

    #endregion

    #region Get Container Logs

    /// <summary>
    /// Retrieve the logs of a specific container as a stream.
    /// </summary>
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

    #region Inspect Container

    /// <summary>
    /// Inspect detailed information about a container.
    /// </summary>
    public async Task<ContainerInspectResponse> InspectContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        return await DockerClient.Containers.InspectContainerAsync(containerId, cancellationToken);
    }

    #endregion
}