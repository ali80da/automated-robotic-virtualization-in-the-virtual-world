using Auto.Core.DataMo.ServiceInfo;
using Auto.Core.Services.StatusChecker;
using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Controllers;

public class HomeController : SharedController
{
   

    [HttpGet("land")]
    public IActionResult Land()
    {
        return View();
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        
        return View();
    }











    
}