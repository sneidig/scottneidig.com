using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// The portfolio. Every project gets its own URL so it can rank for its own terms, and
/// every category filter is a real page rather than a query string, for the same reason.
/// </summary>
[Route("work")]
public class WorkController : Controller
{
    private readonly IProjectService _projects;
    private readonly ICategoryService _categories;

    public WorkController(IProjectService projects, ICategoryService categories)
    {
        _projects = projects;
        _categories = categories;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Work";
        ViewData["Description"] =
            "Websites and nopCommerce work built by Scott Neidig, a web developer on the Colorado Front Range.";

        return View(new WorkListViewModel
        {
            Projects = await _projects.GetPublishedAsync(ct: ct),
            Categories = await _categories.GetWithPublishedProjectsAsync(ct)
        });
    }

    /// <summary>
    /// Sits at /work/category/{slug} rather than /work/{slug} so a category can never
    /// collide with a project of the same name. Both slugs are unique in their own table
    /// but nothing stops "games" being both.
    /// </summary>
    [HttpGet("category/{slug}")]
    public async Task<IActionResult> Category(string slug, CancellationToken ct)
    {
        var category = await _categories.GetBySlugAsync(slug, ct);

        // An empty category is a 404 rather than an empty page. There's nothing there to
        // read, and a thin page that exists is worse for the site than one that doesn't.
        if (category is null || category.ProjectCount == 0)
        {
            return NotFound();
        }

        ViewData["Title"] = category.Name;
        ViewData["Description"] = $"{category.Name} work built by Scott Neidig.";

        return View(nameof(Index), new WorkListViewModel
        {
            Projects = await _projects.GetPublishedAsync(category.Slug, ct: ct),
            Categories = await _categories.GetWithPublishedProjectsAsync(ct),
            SelectedCategory = category
        });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug, CancellationToken ct)
    {
        var project = await _projects.GetPublishedBySlugAsync(slug, ct);
        if (project is null)
        {
            return NotFound();
        }

        ViewData["Title"] = project.PageTitle;
        ViewData["Description"] = project.PageDescription;

        return View(project);
    }
}
