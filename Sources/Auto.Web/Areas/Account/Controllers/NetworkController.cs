using Auto.Core.DataMo.Docker;
using Auto.Core.Services.Docker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.Account.Controllers;

public class NetworkController(IDockerService DockerService) : SharedAccController
{

    private readonly IDockerService DockerService = DockerService;


    #region Networks

    [HttpGet("networks")]
    public async Task<IActionResult> Networks(CancellationToken ct)
    {
        var networks = await DockerService.ListNetworksAsync(ct);
        var model = networks.Select(n => new NetworkViewModel
        {
            ID = n.ID,
            Name = n.Name,
            Driver = n.Driver,
            Scope = n.Scope
        }).ToList();

        return View(model);
    }

    [HttpPost("networks/create")]
    public async Task<IActionResult> CreateNetwork(string name, string driver, CancellationToken ct)
    {
        await DockerService.CreateNetworkAsync(name, driver, ct);
        return RedirectToAction("Networks");
    }

    [HttpPost("networks/remove/{id}")]
    public async Task<IActionResult> RemoveNetwork(string id, CancellationToken ct)
    {
        await DockerService.RemoveNetworkAsync(id, ct);
        return RedirectToAction("Networks");
    }

    [HttpPost("networks/connect")]
    public async Task<IActionResult> ConnectContainer(string containerId, string networkId, CancellationToken ct)
    {
        await DockerService.ConnectContainerToNetworkAsync(containerId, networkId, ct);
        return RedirectToAction("Networks");
    }

    #endregion

}
