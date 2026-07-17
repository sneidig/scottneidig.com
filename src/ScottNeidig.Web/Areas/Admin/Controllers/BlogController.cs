using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Areas.Admin.Models;
using ScottNeidig.Web.Data.Entities;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class BlogController : Controller
{
    private readonly IBlogService _blog;

    public BlogController(IBlogService blog) => _blog = blog;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Blog";
        return View(await _blog.GetAllAsync(ct));
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "New post";
        return View("Form", new BlogPostFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "New post";

        var slug = ResolveSlug(model);

        if (!await IsValidAsync(model, slug, ct))
        {
            return View("Form", model);
        }

        await _blog.CreateAsync(ToEntity(model, slug), ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var post = await _blog.GetByIdAsync(id, ct);
        if (post is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit post";

        return View("Form", new BlogPostFormModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            MarkdownBody = post.MarkdownBody,
            Excerpt = post.Excerpt,
            Published = post.Published,
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPostFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit post";
        model.Id = id;

        var slug = ResolveSlug(model);

        if (!await IsValidAsync(model, slug, ct))
        {
            return View("Form", model);
        }

        var existing = await _blog.GetByIdAsync(id, ct);
        if (existing is null)
        {
            return NotFound();
        }

        var entity = ToEntity(model, slug);
        entity.Id = id;
        // Carry the original publish timestamp; PublishedUtc is stamped on first publish only.
        entity.PublishedUtc = ResolvePublishedUtc(existing, model.Published);

        return await _blog.UpdateAsync(entity, ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await _blog.DeleteAsync(id, ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();

    /// <summary>Blank slug means the create case, so generate from the title; else keep it.</summary>
    private static string ResolveSlug(BlogPostFormModel model) =>
        string.IsNullOrWhiteSpace(model.Slug)
            ? SlugGenerator.Generate(model.Title)
            : SlugGenerator.Generate(model.Slug);

    private static BlogPost ToEntity(BlogPostFormModel model, string slug) => new()
    {
        Slug = slug,
        Title = model.Title.Trim(),
        MarkdownBody = model.MarkdownBody ?? "",
        Excerpt = model.Excerpt?.Trim(),
        Published = model.Published,
        SeoTitle = model.SeoTitle?.Trim(),
        SeoDescription = model.SeoDescription?.Trim()
    };

    /// <summary>
    /// Stamp PublishedUtc the first time a post goes public and keep it after. Unpublishing
    /// clears it, so re-publishing later stamps a fresh date. Now() is fine here: it's a
    /// content timestamp, not something that needs to be UTC-perfect to the millisecond.
    /// </summary>
    private static DateTime? ResolvePublishedUtc(BlogPost existing, bool published)
    {
        if (!published)
        {
            return null;
        }

        return existing.PublishedUtc ?? DateTime.UtcNow;
    }

    private async Task<bool> IsValidAsync(BlogPostFormModel model, string slug, CancellationToken ct)
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

        if (await _blog.SlugExistsAsync(slug, model.Id, ct))
        {
            ModelState.AddModelError(nameof(model.Slug), $"Another post already uses the URL \"{slug}\".");
            return false;
        }

        return true;
    }
}
