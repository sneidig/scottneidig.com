using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>How many projects the home page shows before sending you to /work.</summary>
    private const int FeaturedCount = 3;

    /// <summary>How many posts the "Selected reading" strip shows before sending you to /blog.</summary>
    private const int RecentPostCount = 3;

    private readonly IProjectService _projects;
    private readonly IBlogService _blog;

    public HomeController(IProjectService projects, IBlogService blog)
    {
        _projects = projects;
        _blog = blog;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Title describes the person, not a territory. The local terms came off with the rest
        // of the location signals when the site became a portfolio.
        ViewData["Title"] = "Web and application developer";
        ViewData["Description"] =
            "Scott Neidig, a web and application developer. nopCommerce stores, .NET applications, and business websites, built since 2005.";

        return View(new HomeViewModel
        {
            FeaturedProjects = await _projects.GetPublishedAsync(take: FeaturedCount, ct: ct),
            RecentPosts = await _blog.GetPublishedAsync(take: RecentPostCount, ct: ct)
        });
    }
}
