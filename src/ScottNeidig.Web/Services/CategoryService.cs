using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db) => _db = db;

    public Task<List<CategorySummary>> GetAllAsync(CancellationToken ct = default) =>
        _db.Categories
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategorySummary(
                c.Id, c.Name, c.Slug, c.SortOrder, c.Projects.Count, c.ServiceKey))
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<CategorySummary>> GetWithPublishedProjectsAsync(CancellationToken ct = default) =>
        _db.Categories
            .Where(c => c.Projects.Any(p => p.Published))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            // The count is published-only too. The admin list counts drafts as well, which
            // is right there and wrong here.
            .Select(c => new CategorySummary(
                c.Id, c.Name, c.Slug, c.SortOrder, c.Projects.Count(p => p.Published)))
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<CategorySummary?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Categories
            .Where(c => c.Slug == slug)
            .Select(c => new CategorySummary(
                c.Id, c.Name, c.Slug, c.SortOrder, c.Projects.Count(p => p.Published)))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

    public Task<CategorySummary?> GetByServiceKeyAsync(string serviceKey, CancellationToken ct = default) =>
        _db.Categories
            .Where(c => c.ServiceKey == serviceKey)
            .Select(c => new CategorySummary(
                c.Id, c.Name, c.Slug, c.SortOrder, c.Projects.Count(p => p.Published), c.ServiceKey))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<int> CreateAsync(string name, int sortOrder, string? serviceKey, CancellationToken ct = default)
    {
        var category = new Category
        {
            Name = name.Trim(),
            Slug = SlugGenerator.Generate(name),
            SortOrder = sortOrder,
            ServiceKey = serviceKey
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        return category.Id;
    }

    public async Task<bool> UpdateAsync(
        int id, string name, int sortOrder, string? serviceKey, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct);
        if (category is null)
        {
            return false;
        }

        category.Name = name.Trim();
        // The slug follows the name. Renaming a category changes its URL, which is
        // acceptable here because categories are filters, not indexed landing pages.
        category.Slug = SlugGenerator.Generate(name);
        category.SortOrder = sortOrder;
        category.ServiceKey = serviceKey;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct);
        if (category is null)
        {
            return false;
        }

        // Projects in this category are not deleted. The FK is configured to null out
        // instead, so the work survives.
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default) =>
        _db.Categories.AnyAsync(c => c.Slug == slug && (excludingId == null || c.Id != excludingId), ct);
}
