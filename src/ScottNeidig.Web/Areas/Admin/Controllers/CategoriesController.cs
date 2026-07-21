using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScottNeidig.Web.Areas.Admin.Models;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories) => _categories = categories;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Categories";
        return View(await _categories.GetAllAsync(ct));
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "New category";

        var model = new CategoryFormModel();
        PopulateServiceOptions(model);

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "New category";

        if (!await IsValidAsync(model, ct))
        {
            PopulateServiceOptions(model);
            return View("Form", model);
        }

        await _categories.CreateAsync(model.Name, model.SortOrder, Normalize(model.ServiceKey), ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var category = await _categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit category";

        var model = new CategoryFormModel
        {
            Id = category.Id,
            Name = category.Name,
            SortOrder = category.SortOrder,
            Slug = category.Slug,
            ServiceKey = category.ServiceKey
        };

        PopulateServiceOptions(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit category";
        model.Id = id;

        if (!await IsValidAsync(model, ct))
        {
            PopulateServiceOptions(model);
            return View("Form", model);
        }

        return await _categories.UpdateAsync(id, model.Name, model.SortOrder, Normalize(model.ServiceKey), ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        // Projects in this category are kept; the FK nulls out. See AppDbContext.
        return await _categories.DeleteAsync(id, ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    /// <summary>
    /// Catches a duplicate slug before saving so the user sees a field error rather than
    /// a unique-index violation from SQL Server.
    /// </summary>
    private async Task<bool> IsValidAsync(CategoryFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        var slug = SlugGenerator.Generate(model.Name);

        if (string.IsNullOrEmpty(slug))
        {
            ModelState.AddModelError(nameof(model.Name), "That name doesn't produce a usable URL.");
            return false;
        }

        if (await _categories.SlugExistsAsync(slug, model.Id, ct))
        {
            ModelState.AddModelError(nameof(model.Name), $"Another category already uses the URL \"{slug}\".");
            return false;
        }

        // Each service page shows one category, so catch a double assignment here rather than
        // letting the unique index throw.
        var serviceKey = Normalize(model.ServiceKey);
        if (serviceKey is not null)
        {
            var holder = await _categories.GetByServiceKeyAsync(serviceKey, ct);
            if (holder is not null && holder.Id != model.Id)
            {
                ModelState.AddModelError(
                    nameof(model.ServiceKey),
                    $"\"{holder.Name}\" already feeds that service page. Clear it there first.");
                return false;
            }
        }

        return true;
    }

    /// <summary>Empty selection posts as an empty string; store null so the index ignores it.</summary>
    private static string? Normalize(string? serviceKey) =>
        string.IsNullOrWhiteSpace(serviceKey) ? null : serviceKey;

    private static void PopulateServiceOptions(CategoryFormModel model) =>
        model.ServiceOptions = ServicePages.All
            .Select(pair => new SelectListItem(pair.Value, pair.Key))
            .ToList();
}
