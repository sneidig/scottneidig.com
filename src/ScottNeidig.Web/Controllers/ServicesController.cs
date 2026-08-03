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

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "What I do";
        ViewData["Description"] =
            "The kinds of projects I work on: nopCommerce development, .NET applications, and business websites.";

        return View();
    }

    [HttpGet("nopcommerce")]
    public async Task<IActionResult> NopCommerce(CancellationToken ct)
    {
        ViewData["Title"] = "nopCommerce development";
        ViewData["Description"] =
            "Custom plugins, integrations, upgrades and fixes on live nopCommerce stores. Certified nopCommerce developer.";

        return View(await BuildAsync(ServicePages.NopCommerce, ct));
    }

    [HttpGet("dotnet-development")]
    public async Task<IActionResult> DotNet(CancellationToken ct)
    {
        ViewData["Title"] = ".NET application development";
        ViewData["Description"] =
            "Maintenance, fixes, features and integrations on existing .NET applications, plus new builds. Building on .NET since 2005.";

        return View(await BuildAsync(ServicePages.DotNet, ct));
    }

    [HttpGet("small-business-websites")]
    public async Task<IActionResult> SmallBusiness(CancellationToken ct)
    {
        ViewData["Title"] = "Business websites";
        ViewData["Description"] =
            "Fast, server-rendered business websites that search engines can read without running JavaScript.";

        return View(await BuildAsync(ServicePages.SmallBusiness, ct));
    }

    /// <summary>
    /// Finds the category assigned to this service page and its top projects. When no category
    /// is assigned the model comes back empty and the related-work section simply doesn't render.
    /// </summary>
    private async Task<ServicePageModel> BuildAsync(string serviceKey, CancellationToken ct)
    {
        var category = await _categories.GetByServiceKeyAsync(serviceKey, ct);
        if (category is null)
        {
            return new ServicePageModel();
        }

        return new ServicePageModel
        {
            CategoryName = category.Name,
            CategorySlug = category.Slug,
            Projects = await _projects.GetPublishedAsync(category.Slug, RelatedCount, ct),
            Posts = await _blog.GetPublishedByCategoryAsync(category.Slug, RelatedCount, ct: ct)
        };
    }
}
