using Auto.Core.Services.Docker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.Account.Controllers;

public class ImageController(IDockerService DockerService) : SharedAccController
{

    private readonly IDockerService DockerService = DockerService;

    #region List Images

    [HttpGet("images")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var images = await DockerService.ListImagesAsync(cancellationToken);
        return View(images);
    }

    #endregion

    #region Pull Image

    [HttpPost("pull-image")]
    public async Task<IActionResult> Pull(string image, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(image))
            return BadRequest("Image name required");

        await DockerService.PullImageAsync(image, ct);
        return RedirectToAction("Index");
    }

    #endregion

    #region Remove Image

    [HttpPost("remove-image/{id}")]
    public async Task<IActionResult> Remove(string id, CancellationToken ct)
    {
        await DockerService.RemoveImageAsync(id, true, ct);
        return RedirectToAction("Index");
    }

    #endregion



}