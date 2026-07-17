using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>How many projects the home page shows before sending you to /work.</summary>
    private const int FeaturedCount = 3;

    private readonly IProjectService _projects;

    public HomeController(IProjectService projects) => _projects = projects;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Title carries the primary local term ("Boulder web developer") since it's the
        // strongest on-page ranking signal. Kept natural, not stuffed.
        ViewData["Title"] = "Web Developer in Boulder, CO";
        ViewData["Description"] =
            "Scott Neidig, a web and application developer in Boulder, Colorado. nopCommerce and .NET work, and websites for small businesses across the Denver metro and Front Range.";

        return View(new HomeViewModel
        {
            FeaturedProjects = await _projects.GetPublishedAsync(take: FeaturedCount, ct: ct)
        });
    }
}
