using Microsoft.AspNetCore.Mvc;

namespace Auto.Web.Areas.V01.Account.Controllers;

public class TerminalController : SharedAcc01Controller
{



    [HttpGet("terminal")]
    public IActionResult Terminal(string id)
    {
        return View("Terminal", id);
    }

















}