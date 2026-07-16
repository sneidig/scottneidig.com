using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db) => _db = db;

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

        // Images and points go with it: the FKs are configured to cascade.
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default) =>
        _db.Projects.AnyAsync(p => p.Slug == slug && (excludingId == null || p.Id != excludingId), ct);
}
