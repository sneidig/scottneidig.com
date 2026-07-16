using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return View("Form", new CategoryFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "New category";

        if (!await IsValidAsync(model, ct))
        {
            return View("Form", model);
        }

        await _categories.CreateAsync(model.Name, model.SortOrder, ct);
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

        return View("Form", new CategoryFormModel
        {
            Id = category.Id,
            Name = category.Name,
            SortOrder = category.SortOrder,
            Slug = category.Slug
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit category";
        model.Id = id;

        if (!await IsValidAsync(model, ct))
        {
            return View("Form", model);
        }

        return await _categories.UpdateAsync(id, model.Name, model.SortOrder, ct)
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

        return true;
    }
}
