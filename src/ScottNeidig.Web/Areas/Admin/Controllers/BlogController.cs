using System.IO.Compression;
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
public class BlogController : Controller
{
    /// <summary>Image types allowed out of an imported zip. SVG is the usual one for diagrams.</summary>
    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".svg", ".png", ".jpg", ".jpeg", ".webp", ".gif" };

    private readonly IBlogService _blog;
    private readonly IBlogImageStorage _images;
    private readonly ICategoryService _categories;
    private readonly ILogger<BlogController> _log;

    public BlogController(
        IBlogService blog, IBlogImageStorage images, ICategoryService categories, ILogger<BlogController> log)
    {
        _blog = blog;
        _images = images;
        _categories = categories;
        _log = log;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Blog";
        return View(await _blog.GetAllAsync(ct));
    }

    [HttpGet]
    public IActionResult Import()
    {
        ViewData["Title"] = "Import post";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile? zip, CancellationToken ct)
    {
        ViewData["Title"] = "Import post";

        if (zip is null || zip.Length == 0)
        {
            ModelState.AddModelError("", "Choose a zip file to import.");
            return View();
        }

        try
        {
            var id = await ProcessZipAsync(zip, ct);

            // Land on the post's edit form, unpublished, so it gets a look before going live.
            TempData["Message"] = "Imported as a draft. Review it and publish when it's ready.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (ImportException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "New post";

        var model = new BlogPostFormModel();
        await PopulateFormListsAsync(model, ct);

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "New post";

        var slug = ResolveSlug(model);

        if (!await IsValidAsync(model, slug, ct))
        {
            await PopulateFormListsAsync(model, ct);
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

        var model = new BlogPostFormModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            MarkdownBody = post.MarkdownBody,
            Excerpt = post.Excerpt,
            Published = post.Published,
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription,
            CategoryId = post.CategoryId,
            HeroImage = post.HeroImage
        };

        await PopulateFormListsAsync(model, ct);
        return View("Form", model);
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
            await PopulateFormListsAsync(model, ct);
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
        SeoDescription = model.SeoDescription?.Trim(),
        CategoryId = model.CategoryId,
        HeroImage = string.IsNullOrWhiteSpace(model.HeroImage) ? null : model.HeroImage
    };

    private async Task PopulateFormListsAsync(BlogPostFormModel model, CancellationToken ct)
    {
        var categories = await _categories.GetAllAsync(ct);
        model.Categories = categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();

        // Hero options are the post's own uploaded images. New posts have none yet, so the
        // picker is empty until images are imported.
        var slug = model.Slug;
        model.AvailableImages = string.IsNullOrWhiteSpace(slug)
            ? []
            : _images.ListForPost(slug)
                .Select(name => new SelectListItem(name, name))
                .ToList();
    }

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

    /// <summary>
    /// Reads the zip, upserts the post by slug, and stores its images. Re-importing the same slug
    /// updates the existing post in place (its published state is kept), which is the edit path:
    /// regenerate the draft, drop the zip again. New posts come in unpublished.
    /// </summary>
    private async Task<int> ProcessZipAsync(IFormFile zip, CancellationToken ct)
    {
        using var archive = OpenArchive(zip);

        // The single .md entry is the post; anything with an image extension is an image.
        var markdownEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            ?? throw new ImportException("The zip has no .md file in it.");

        var markdown = await ReadEntryText(markdownEntry, ct);

        var fields = BlogImportParser.ParseFields(markdown);
        var slug = SlugGenerator.Generate(fields.Slug ?? fields.Title);
        if (string.IsNullOrEmpty(slug))
        {
            throw new ImportException("The draft is missing a Slug and Title, so it has no URL.");
        }

        if (string.IsNullOrWhiteSpace(fields.Title))
        {
            throw new ImportException("The draft is missing a Title.");
        }

        var body = BlogImportParser.RewriteImagePaths(BlogImportParser.StripFrontMatter(markdown), slug);

        // Clear any previous images for this slug before writing the new set, so a removed or
        // renamed image can't linger and get served.
        _images.DeleteAllForPost(slug);

        var imageCount = 0;
        string? firstImage = null;
        foreach (var entry in archive.Entries)
        {
            if (entry.Length == 0 || !ImageExtensions.Contains(Path.GetExtension(entry.Name)))
            {
                continue;
            }

            using var stream = entry.Open();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, ct);
            await _images.SaveAsync(slug, entry.Name, buffer.ToArray(), ct);
            firstImage ??= Path.GetFileName(entry.Name);
            imageCount++;
        }

        var existing = await _blog.GetBySlugForEditAsync(slug, ct);
        int id;

        if (existing is null)
        {
            id = await _blog.CreateAsync(new BlogPost
            {
                Slug = slug,
                Title = fields.Title.Trim(),
                MarkdownBody = body,
                Excerpt = fields.Excerpt?.Trim(),
                SeoTitle = fields.SeoTitle?.Trim(),
                SeoDescription = fields.SeoDescription?.Trim(),
                Published = false,
                // Default the hero to the first image; it's a pick you can change on review.
                HeroImage = firstImage
            }, ct);
        }
        else
        {
            existing.Title = fields.Title.Trim();
            existing.MarkdownBody = body;
            existing.Excerpt = fields.Excerpt?.Trim();
            existing.SeoTitle = fields.SeoTitle?.Trim();
            existing.SeoDescription = fields.SeoDescription?.Trim();
            // Keep the chosen hero if that image is still in the new set, else fall back to the
            // first image so a re-import never leaves a hero pointing at a deleted file.
            var imagesNow = _images.ListForPost(slug);
            if (existing.HeroImage is null || !imagesNow.Contains(existing.HeroImage))
            {
                existing.HeroImage = firstImage;
            }
            // Published state and its date are left as they are: re-importing a live post updates
            // it without pulling it down.
            await _blog.UpdateAsync(existing, ct);
            id = existing.Id;
        }

        _log.LogInformation("Imported blog post {Slug} with {ImageCount} images", slug, imageCount);
        return id;
    }

    private static ZipArchive OpenArchive(IFormFile zip)
    {
        try
        {
            return new ZipArchive(zip.OpenReadStream(), ZipArchiveMode.Read);
        }
        catch (InvalidDataException)
        {
            throw new ImportException("That file isn't a valid zip.");
        }
    }

    private static async Task<string> ReadEntryText(ZipArchiveEntry entry, CancellationToken ct)
    {
        using var reader = new StreamReader(entry.Open());
        return await reader.ReadToEndAsync(ct);
    }

    /// <summary>A problem with the uploaded file that's the user's to fix, shown on the form.</summary>
    private sealed class ImportException(string message) : Exception(message);
}
