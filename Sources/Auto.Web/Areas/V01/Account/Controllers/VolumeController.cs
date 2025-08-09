using Auto.Core.DataMo.Docker;
using Auto.Core.Services.Docker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.V01.Account.Controllers;

public class VolumeController(IDockerService DockerService) : SharedAcc01Controller
{
    private readonly IDockerService DockerService = DockerService;

    #region Volumes

    [HttpGet("volumes")]
    public async Task<IActionResult> Volumes(CancellationToken ct)
    {
        var volumes = await DockerService.ListVolumesAsync(ct);
        var model = volumes.Select(v => new VolumeViewModel
        {
            Name = v.Name,
            Driver = v.Driver,
            Mountpoint = v.Mountpoint
        }).ToList();

        return View(model);
    }

    [HttpPost("volumes/remove/{name}")]
    public async Task<IActionResult> RemoveVolume(string name, CancellationToken ct)
    {
        await DockerService.RemoveVolumeAsync(name, ct);
        return RedirectToAction("Volumes");
    }

    [HttpPost("volumes/prune")]
    public async Task<IActionResult> PruneVolumes(CancellationToken ct)
    {
        await DockerService.PruneVolumesAsync(ct);
        return RedirectToAction("Volumes");
    }

    #endregion
}