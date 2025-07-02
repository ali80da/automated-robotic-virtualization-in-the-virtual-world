using System;

namespace Auto.Core.DataMo.Docker;

#region ViewModels

public class ContainerViewModel
{
    #region Identification

    /// <summary>
    /// Unique container ID (full hash).
    /// </summary>
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the container (from Docker name list).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Docker image used to create this container.
    /// </summary>
    public string Image { get; set; } = string.Empty;

    #endregion

    #region State and Lifecycle

    /// <summary>
    /// Current runtime status (e.g. running, exited).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Container creation time in user-friendly format.
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Time since the container started running (if applicable).
    /// </summary>
    public string Uptime { get; set; } = string.Empty;

    #endregion

    #region Configuration and Resources

    /// <summary>
    /// Networking mode (e.g. bridge, host).
    /// </summary>
    public string? NetworkMode { get; set; }

    /// <summary>
    /// Published ports and mappings.
    /// </summary>
    public List<string> Ports { get; set; } = new();

    /// <summary>
    /// Total root filesystem size (e.g. "150MB").
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Current CPU usage snapshot.
    /// </summary>
    public string? CPUUsage { get; set; }

    /// <summary>
    /// Current memory usage snapshot.
    /// </summary>
    public string? MemoryUsage { get; set; }

    #endregion
}


public class DashboardViewModel
{
    public int TotalContainers { get; set; }
    public int Running { get; set; }
    public int Exited { get; set; }
    public Dictionary<string, int>? ImageUsage { get; set; }

    public List<ContainerViewModel> Containers { get; set; } = new();

    // Filters
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
    public string? ImageFilter { get; set; }

    public List<string> AvailableImages { get; set; } = new();
}

#endregion