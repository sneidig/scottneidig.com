using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public interface ICategoryService
{
    /// <summary>Ordered for display, with project counts, in one query.</summary>
    Task<List<CategorySummary>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Categories that have at least one published project. The public filter nav is built
    /// from this, so it never offers a link to an empty page.
    /// </summary>
    Task<List<CategorySummary>> GetWithPublishedProjectsAsync(CancellationToken ct = default);

    /// <summary>Null when the slug is unknown. The caller turns that into a 404.</summary>
    Task<CategorySummary?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Returns the new id.</summary>
    Task<int> CreateAsync(string name, int sortOrder, CancellationToken ct = default);

    /// <summary>False when the category no longer exists.</summary>
    Task<bool> UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// True when another category already uses this slug. Checked before saving so the
    /// user gets a validation message instead of a unique-index exception.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default);
}
