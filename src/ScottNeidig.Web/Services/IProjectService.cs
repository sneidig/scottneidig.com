using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public interface IProjectService
{
    /// <summary>Every project, published or not, for the admin list.</summary>
    Task<List<ProjectSummary>> GetAllAsync(CancellationToken ct = default);

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
