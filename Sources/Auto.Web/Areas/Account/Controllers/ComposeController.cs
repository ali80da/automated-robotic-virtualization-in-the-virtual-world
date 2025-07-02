using Auto.Core.DataMo.Docker;
using Auto.Core.Services.Docker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.Account.Controllers;

public class ComposeController(IDockerService DockerService) : SharedAccController
{

    private readonly IDockerService DockerService = DockerService;

    /// <summary>
    /// Shows the initial view for Docker Compose management.
    /// </summary>
    /// <returns>Index view with empty YAML model.</returns>
    [HttpGet("compose")]
    public IActionResult Index() => View(new DockerComposeViewModel());

    /// <summary>
    /// Parses the docker-compose YAML and shows the list of defined services.
    /// </summary>
    /// <param name="model">The view model containing YAML content.</param>
    /// <returns>The same view with parsed services populated.</returns>
    [HttpPost("parse")]
    public async Task<IActionResult> Parse(DockerComposeViewModel model)
    {
        model.Services = await DockerService.ParseComposeServicesAsync(model.YamlContent);
        return View("Index", model);
    }

    /// <summary>
    /// Executes 'docker compose up -d' to launch the services.
    /// </summary>
    /// <param name="model">The view model containing YAML content.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>Redirects back to index.</returns>
    [HttpPost("run")]
    public async Task<IActionResult> Run(DockerComposeViewModel model, CancellationToken ct)
    {
        await DockerService.RunComposeAsync(model.YamlContent, ct);
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Executes 'docker compose down' to stop and remove the services.
    /// </summary>
    /// <param name="model">The view model containing YAML content.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>Redirects back to index.</returns>
    [HttpPost("stop")]
    public async Task<IActionResult> Stop(DockerComposeViewModel model, CancellationToken ct)
    {
        await DockerService.StopComposeAsync(model.YamlContent, ct);
        return RedirectToAction("Index");
    }

}