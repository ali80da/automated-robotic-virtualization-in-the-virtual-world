using System;
namespace Auto.Core.DataMo.Docker;

/// <summary>
/// ViewModel for managing docker-compose YAML and service parsing.
/// </summary>
public class DockerComposeViewModel
{
    /// <summary>
    /// The raw content of the docker-compose.yml file.
    /// </summary>
    public string YamlContent { get; set; } = string.Empty;

    /// <summary>
    /// Parsed list of service names from the YAML.
    /// </summary>
    public string[]? Services { get; set; }
}