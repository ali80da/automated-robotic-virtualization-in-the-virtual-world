using System;

namespace Auto.Core.DataMo.Docker;

/// <summary>
/// Represents a simplified network model for view rendering.
/// </summary>
public class NetworkViewModel
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}