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

    // Portfolio hidden while not in use. Restore by reverting this controller.

    [HttpGet("")]
    public IActionResult Index() => NotFound();

    [HttpGet("category/{slug}")]
    public IActionResult Category(string slug) => NotFound();

    [HttpGet("{slug}")]
    public IActionResult Detail(string slug) => NotFound();
}
