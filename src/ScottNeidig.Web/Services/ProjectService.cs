using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly IImageStorage _storage;

    public ProjectService(AppDbContext db, IImageStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public Task<List<ProjectSummary>> GetAllAsync(CancellationToken ct = default) =>
        _db.Projects
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Title)
            .Select(p => new ProjectSummary(
                p.Id,
                p.Title,
                p.Slug,
                p.Category != null ? p.Category.Name : null,
                p.SortOrder,
                p.Published,
                p.Images.Count,
                p.Points.Count))
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<ProjectCard>> GetPublishedAsync(
        string? categorySlug = null, int? take = null, CancellationToken ct = default)
    {
        var query = _db.Projects.Where(p => p.Published);

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p => p.Category != null && p.Category.Slug == categorySlug);
        }

        var ordered = query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Title)
            .Select(p => new ProjectCard(
                p.Slug,
                p.Title,
                p.Summary,
                p.MetaText,
                p.Category != null ? p.Category.Name : null,
                // Same hero rule as the detail page: the flagged one, else the first.
                // Ordered inside the projection so it stays one query instead of one per row.
                p.Images
                    .OrderByDescending(i => i.IsHero)
                    .ThenBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => i.FileName)
                    .FirstOrDefault(),
                p.Images
                    .OrderByDescending(i => i.IsHero)
                    .ThenBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => i.Caption)
                    .FirstOrDefault()))
            .AsNoTracking();

        return take is > 0
            ? ordered.Take(take.Value).ToListAsync(ct)
            : ordered.ToListAsync(ct);
    }

    public Task<ProjectDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Projects
            .Where(p => p.Published && p.Slug == slug)
            .Select(p => new ProjectDetail(
                p.Slug,
                p.Title,
                p.Summary,
                p.MetaText,
                p.LiveUrl,
                p.Category != null ? p.Category.Name : null,
                p.Category != null ? p.Category.Slug : null,
                p.SeoTitle,
                p.SeoDescription,
                p.Points
                    .OrderBy(pt => pt.SortOrder)
                    .ThenBy(pt => pt.Id)
                    .Select(pt => new ProjectPointSummary(pt.Id, pt.Title, pt.Body, pt.SortOrder))
                    .ToList(),
                p.Images
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => new ProjectImageSummary(i.Id, i.FileName, i.Caption, i.SortOrder, i.IsHero))
                    .ToList()))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

    /// <summary>Tracked, because the caller edits and saves it.</summary>
    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<int> CreateAsync(Project project, CancellationToken ct = default)
    {
        project.CreatedUtc = DateTime.UtcNow;

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        return project.Id;
    }

    public async Task<bool> UpdateAsync(Project project, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(project.Id, ct);
        if (existing is null)
        {
            return false;
        }

        // Copied field by field rather than attaching the incoming instance, so a
        // stale or absent CreatedUtc can't overwrite the real one, and so the
        // Images/Points collections are left alone. Those are managed separately.
        existing.Slug = project.Slug;
        existing.Title = project.Title;
        existing.Summary = project.Summary;
        existing.MetaText = project.MetaText;
        existing.LiveUrl = project.LiveUrl;
        existing.CategoryId = project.CategoryId;
        existing.SortOrder = project.SortOrder;
        existing.Published = project.Published;
        existing.SeoTitle = project.SeoTitle;
        existing.SeoDescription = project.SeoDescription;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await GetByIdAsync(id, ct);
        if (project is null)
        {
            return false;
        }

        // The FK cascade takes the image rows with the project, which would leave their
        // files orphaned on disk with nothing pointing at them. Read the names while the
        // rows still exist.
        var fileNames = await _db.ProjectImages
            .Where(i => i.ProjectId == id)
            .Select(i => i.FileName)
            .ToListAsync(ct);

        // Images and points go with it: the FKs are configured to cascade.
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);

        // Files last, once the delete has committed. The other order risks binning the
        // files and then failing to save, leaving rows pointing at nothing.
        foreach (var fileName in fileNames)
        {
            _storage.Delete(fileName);
        }

        return true;
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default) =>
        _db.Projects.AnyAsync(p => p.Slug == slug && (excludingId == null || p.Id != excludingId), ct);
}
