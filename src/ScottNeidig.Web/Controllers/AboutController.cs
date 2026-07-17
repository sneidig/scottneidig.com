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
            "Scott Neidig, a web and application developer on the Colorado Front Range with 25 years of .NET experience.";

        return View();
    }
}
