using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// The three kinds of work, one page each, describing what the work is and what backs it.
/// These are portfolio pages, not offers: no CTA, no pricing, nothing that asks the reader
/// for anything. The copy lives in the views; the related projects and posts are pulled from
/// whichever category is assigned to that page in the admin.
///
/// The routes still read /services because they are the indexed URLs. Only the framing changed.
/// </summary>
[Route("services")]
public class ServicesController : Controller
{
    /// <summary>How many projects a service page shows before linking to the full category.</summary>
    private const int RelatedCount = 3;

    private readonly ICategoryService _categories;
    private readonly IProjectService _projects;
    private readonly IBlogService _blog;

    public ServicesController(ICategoryService categories, IProjectService projects, IBlogService blog)
    {
        _categories = categories;
        _projects = projects;
        _blog = blog;
    }

    // Services pages hidden while not in use. Restore by reverting this controller.

    [HttpGet("")]
    public IActionResult Index() => NotFound();

    [HttpGet("nopcommerce")]
    public IActionResult NopCommerce() => NotFound();

    [HttpGet("dotnet-development")]
    public IActionResult DotNet() => NotFound();

    [HttpGet("small-business-websites")]
    public IActionResult SmallBusiness() => NotFound();
}
