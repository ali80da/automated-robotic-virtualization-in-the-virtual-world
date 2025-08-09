using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.V01.Account.Controllers;

public class DashboardController : SharedAcc01Controller
{




    public IActionResult Index()
    {
        return View();
    }



}