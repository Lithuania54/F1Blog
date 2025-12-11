using Microsoft.AspNetCore.Mvc;

namespace F1.Web.Controllers;

public class F1GameController : Controller
{
    [HttpGet("/F1Game")]
    public IActionResult Index()
    {
        ViewData["Title"] = "F1 Game";
        return View();
    }
}
