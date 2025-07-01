using System.Runtime.InteropServices;
using Docker.DotNet;

namespace Auto.Core.Extensions.StaticExtensions;

public static class DockerClientFactory
{
    public static DockerClient Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new DockerClientConfiguration(
                new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        throw new PlatformNotSupportedException("Unsupported OS for Docker client.");
    }

}