using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.Account.Controllers;

public class TerminalController : SharedAccController
{



    [HttpGet("terminal")]
    public IActionResult Terminal(string id)
    {
        return View("Terminal", id);
    }

















}