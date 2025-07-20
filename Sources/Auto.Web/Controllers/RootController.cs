using Auto.Core.Services.Docker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Controllers;

public class RootController : SharedController
{
    private readonly IDockerService DockerService;
    public RootController(IDockerService DockerService) => this.DockerService = DockerService;


    //[HttpGet("{service}")]
    //public async Task<IActionResult> RedirectToService(string service)
    //{
    //    var targetUrl = /*await _checker.GetServiceUrlAsync(service)*/ "";

    //    if (targetUrl != null)
    //        return Redirect(targetUrl);

    //    return Content($"سرویس «{service}» در حال حاضر در دسترس نیست.", "text/plain");
    //}

    [HttpPost]
    public async Task<IActionResult> InstallDocker()
    {
        //await DockerService.InstallDockerAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> StartDocker()
    {
        //await DockerService.StartDockerAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> RestartDocker()
    {
        //await DockerService.RestartDockerAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult RefreshDockerStatus()
    {
        // فقط ری‌لود صفحه کفایت می‌کند (middleware خودش دوباره بررسی می‌کند)
        return RedirectToAction("Index", "Home");
    }



}