using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// The front door. Each service is a landing page whose job is to pitch and send the reader
/// to /contact. The pitch copy lives in the views; the related projects are pulled from
/// whichever category is assigned to that page in the admin.
/// </summary>
[Route("services")]
public class ServicesController : Controller
{
    /// <summary>How many projects a service page shows before linking to the full category.</summary>
    private const int RelatedCount = 3;

    private readonly ICategoryService _categories;
    private readonly IProjectService _projects;

    public ServicesController(ICategoryService categories, IProjectService projects)
    {
        _categories = categories;
        _projects = projects;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Services";
        ViewData["Description"] =
            "nopCommerce development, .NET application support, and small-business websites, by an ex-agency developer.";

        return View();
    }

    [HttpGet("nopcommerce")]
    public async Task<IActionResult> NopCommerce(CancellationToken ct)
    {
        ViewData["Title"] = "nopCommerce development";
        ViewData["Description"] =
            "Ongoing nopCommerce development and support for existing stores, by an ex-agency nopCommerce developer.";

        return View(await BuildAsync(ServicePages.NopCommerce, ct));
    }

    [HttpGet("dotnet-development")]
    public async Task<IActionResult> DotNet(CancellationToken ct)
    {
        ViewData["Title"] = ".NET application development and support";
        ViewData["Description"] =
            "Maintenance, fixes and new features for existing .NET applications, plus new builds. 25 years of .NET.";

        return View(await BuildAsync(ServicePages.DotNet, ct));
    }

    [HttpGet("small-business-websites")]
    public async Task<IActionResult> SmallBusiness(CancellationToken ct)
    {
        ViewData["Title"] = "Small Business Websites in Boulder & Denver";
        ViewData["Description"] =
            "Fast, search-friendly websites for small businesses in Boulder, the Denver metro, and the Colorado Front Range.";

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
            Projects = await _projects.GetPublishedAsync(category.Slug, RelatedCount, ct)
        };
    }
}
