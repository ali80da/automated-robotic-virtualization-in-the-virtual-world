using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.Account.Controllers;

public class DashboardController : SharedAccController
{




    public IActionResult Index()
    {
        return View();
    }



}