using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public interface IProjectService
{
    /// <summary>Every project, published or not, for the admin list.</summary>
    Task<List<ProjectSummary>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Published projects only, for the public site. Separate from GetAllAsync on purpose:
    /// a draft leaking onto a public page is the kind of bug you find out about from
    /// someone else, so the public path can't share a query with the admin one.
    /// </summary>
    /// <param name="categorySlug">Null for everything, otherwise limits to that category.</param>
    /// <param name="take">Null for all, otherwise the first n in display order.</param>
    Task<List<ProjectCard>> GetPublishedAsync(
        string? categorySlug = null, int? take = null, CancellationToken ct = default);

    /// <summary>
    /// Null when the slug is unknown or the project isn't published. The caller turns that
    /// into a 404: an unpublished project must be indistinguishable from one that never
    /// existed, or the URL confirms it's there.
    /// </summary>
    Task<ProjectDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);

    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(Project project, CancellationToken ct = default);

    /// <summary>False when the project no longer exists.</summary>
    Task<bool> UpdateAsync(Project project, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// True when another project already uses this slug. Checked before saving so the
    /// user gets a validation message instead of a unique-index exception.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default);
}
