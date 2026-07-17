using Microsoft.AspNetCore.Mvc;

namespace ScottNeidig.Web.Controllers;

[Route("about")]
public class AboutController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "About";
        ViewData["Description"] =
            "Scott Neidig, a web and application developer in Boulder, Colorado, building .NET applications and nopCommerce stores since 2005.";

        return View();
    }
}
