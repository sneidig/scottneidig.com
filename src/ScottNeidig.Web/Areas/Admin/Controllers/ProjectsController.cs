using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScottNeidig.Web.Areas.Admin.Models;
using ScottNeidig.Web.Data.Entities;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projects;
    private readonly ICategoryService _categories;

    public ProjectsController(IProjectService projects, ICategoryService categories)
    {
        _projects = projects;
        _categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Projects";
        return View(await _projects.GetAllAsync(ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "New project";

        var model = new ProjectFormModel();
        await PopulateCategoriesAsync(model, ct);

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "New project";

        var slug = ResolveSlug(model);

        if (!await IsValidAsync(model, slug, ct))
        {
            await PopulateCategoriesAsync(model, ct);
            return View("Form", model);
        }

        await _projects.CreateAsync(ToEntity(model, slug), ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        if (project is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit project";

        var model = new ProjectFormModel
        {
            Id = project.Id,
            Title = project.Title,
            Slug = project.Slug,
            Summary = project.Summary,
            MetaText = project.MetaText,
            LiveUrl = project.LiveUrl,
            CategoryId = project.CategoryId,
            SortOrder = project.SortOrder,
            Published = project.Published,
            SeoTitle = project.SeoTitle,
            SeoDescription = project.SeoDescription
        };

        await PopulateCategoriesAsync(model, ct);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit project";
        model.Id = id;

        var slug = ResolveSlug(model);

        if (!await IsValidAsync(model, slug, ct))
        {
            await PopulateCategoriesAsync(model, ct);
            return View("Form", model);
        }

        var entity = ToEntity(model, slug);
        entity.Id = id;

        return await _projects.UpdateAsync(entity, ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await _projects.DeleteAsync(id, ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();

    /// <summary>
    /// Falls back to generating from the title only when the slug field is blank, which
    /// is the create case. On edit the existing slug is posted back and kept, so renaming
    /// a project never moves its URL.
    /// </summary>
    private static string ResolveSlug(ProjectFormModel model) =>
        string.IsNullOrWhiteSpace(model.Slug)
            ? SlugGenerator.Generate(model.Title)
            : SlugGenerator.Generate(model.Slug);

    private static Project ToEntity(ProjectFormModel model, string slug) => new()
    {
        Slug = slug,
        Title = model.Title.Trim(),
        Summary = model.Summary?.Trim(),
        MetaText = model.MetaText?.Trim(),
        LiveUrl = model.LiveUrl?.Trim(),
        CategoryId = model.CategoryId,
        SortOrder = model.SortOrder,
        Published = model.Published,
        SeoTitle = model.SeoTitle?.Trim(),
        SeoDescription = model.SeoDescription?.Trim()
    };

    private async Task<bool> IsValidAsync(ProjectFormModel model, string slug, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        if (string.IsNullOrEmpty(slug))
        {
            ModelState.AddModelError(nameof(model.Slug), "That title doesn't produce a usable URL. Set a slug manually.");
            return false;
        }

        if (await _projects.SlugExistsAsync(slug, model.Id, ct))
        {
            ModelState.AddModelError(nameof(model.Slug), $"Another project already uses the URL \"{slug}\".");
            return false;
        }

        return true;
    }

    private async Task PopulateCategoriesAsync(ProjectFormModel model, CancellationToken ct)
    {
        var categories = await _categories.GetAllAsync(ct);

        model.Categories = categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();
    }
}
