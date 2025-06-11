using Auto.Core.DataMo.Common;
using Auto.Core.DataMo.ServiceInfo;
using Auto.Core.Services.StatusChecker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Controllers;

public class ServiceController(
    IStatusCheckerService StatusCheckerService
    ) : Controller
{

    private readonly IStatusCheckerService StatusService = StatusCheckerService;


    /// <summary>
    /// نمایش صفحه لیست سرویس‌ها با آخرین وضعیت‌ها.
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> Index()
    {
        var names = await StatusService.GetAllServiceNames();

        var resultList = new List<StatusResult<ServiceStatusInfo>>();

        foreach (var name in names)
        {
            var result = await StatusService.RefreshAsync(name); // یا TryGetLastStatus
            if (result != null)
                resultList.Add(result);
        }

        return View(resultList);
    }


    /// <summary>
    /// بررسی و نمایش مجدد وضعیت یک سرویس خاص.
    /// </summary>
    [HttpGet("services/status/{name}")]
    public async Task<IActionResult> Status(string name)
    {
        var result = await StatusService.RefreshAsync(name);
        return View("Status", result); // View: Views/Services/Status.cshtml
    }

    /// <summary>
    /// هدایت کاربر به سرویس در صورت فعال بودن.
    /// </summary>
    [HttpGet("/go/{name}")]
    public async Task<IActionResult> Go(string name)
    {
        var result = await StatusService.RefreshAsync(name);

        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data?.Url))
            return Redirect(result.Data.Url);

        var error = result.Message;
        return View("Error", error);
    }

    /// <summary>
    /// ثبت یک سرویس جدید (از form یا ajax).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] ServiceInfo model)
    {
        await StatusService.RegisterServiceAsync(model);
        return RedirectToAction("Index");
    }

}