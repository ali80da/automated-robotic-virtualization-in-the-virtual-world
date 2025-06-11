using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Controllers;

public class AuthController : SharedController
{

    [HttpGet("signin")]
    public IActionResult Signin()
    {
        return View();
    }



}