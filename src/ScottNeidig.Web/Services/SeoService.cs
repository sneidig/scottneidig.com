using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;

namespace ScottNeidig.Web.Services;

public class SeoService : ISeoService
{
    private readonly AppDbContext _db;

    public SeoService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SitemapEntry>> GetSitemapEntriesAsync(CancellationToken ct = default)
    {
        // Static pages, highest priority at the top. These change rarely, so no lastmod.
        var entries = new List<SitemapEntry>
        {
            new("/", Priority: 1.0),
            new("/services", Priority: 0.9),
            new("/services/nopcommerce", Priority: 0.8),
            new("/services/dotnet-development", Priority: 0.8),
            new("/services/small-business-websites", Priority: 0.8),
            new("/work", Priority: 0.8),
            new("/about", Priority: 0.7),
            new("/contact", Priority: 0.7),
        };

        // Published projects, with CreatedUtc as lastmod. Only published, matching the public
        // queries, so a crawler is never sent to a draft's 404.
        var projects = await _db.Projects
            .Where(p => p.Published)
            .OrderBy(p => p.SortOrder)
            .Select(p => new { p.Slug, p.CreatedUtc })
            .AsNoTracking()
            .ToListAsync(ct);

        entries.AddRange(projects.Select(p => new SitemapEntry($"/work/{p.Slug}", p.CreatedUtc, 0.7)));

        // Category filter pages, only those with at least one published project (the empty ones
        // 404 by design).
        var categories = await _db.Categories
            .Where(c => c.Projects.Any(p => p.Published))
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Slug)
            .AsNoTracking()
            .ToListAsync(ct);

        entries.AddRange(categories.Select(slug => new SitemapEntry($"/work/category/{slug}", Priority: 0.6)));

        return entries;
    }
}
