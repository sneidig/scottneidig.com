using System.Diagnostics;
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
        ViewData["Title"] = "Web Developer";
        ViewData["Description"] =
            "Scott Neidig, web developer. Fast, SEO-first websites for small business, and nopCommerce development.";

        return View(new HomeViewModel
        {
            FeaturedProjects = await _projects.GetPublishedAsync(take: FeaturedCount, ct: ct)
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
