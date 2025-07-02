using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto.Core.DataMo.Docker;

/// <summary>
/// Represents a simplified volume model for view rendering.
/// </summary>
public class VolumeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string Mountpoint { get; set; } = string.Empty;
}



