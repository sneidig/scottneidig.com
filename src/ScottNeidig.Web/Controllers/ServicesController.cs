using Microsoft.AspNetCore.Mvc;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// The front door. Each service is a landing page whose job is to pitch and send the reader
/// to /contact. Two static pages for now; when a third service appears this stays one
/// controller with one action per page.
/// </summary>
[Route("services")]
public class ServicesController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Services";
        ViewData["Description"] =
            "nopCommerce development, .NET application support, and small-business websites, by an ex-agency developer.";

        return View();
    }

    [HttpGet("nopcommerce")]
    public IActionResult NopCommerce()
    {
        ViewData["Title"] = "nopCommerce development";
        ViewData["Description"] =
            "Ongoing nopCommerce development and support for existing stores, by an ex-agency nopCommerce developer.";

        return View();
    }

    [HttpGet("dotnet-development")]
    public IActionResult DotNet()
    {
        ViewData["Title"] = ".NET application development and support";
        ViewData["Description"] =
            "Maintenance, fixes and new features for existing .NET applications, plus new builds. 25 years of .NET.";

        return View();
    }

    [HttpGet("small-business-websites")]
    public IActionResult SmallBusiness()
    {
        ViewData["Title"] = "Small business websites";
        ViewData["Description"] =
            "Fast, search-friendly websites for small businesses on the Colorado Front Range.";

        return View();
    }
}
