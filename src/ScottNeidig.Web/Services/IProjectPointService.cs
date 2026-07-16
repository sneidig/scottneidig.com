using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

/// <summary>
/// Points always belong to a project, so every method takes the project id and scopes
/// its query by it. Passing a point id that belongs to a different project returns
/// nothing rather than the wrong project's data.
/// </summary>
public interface IProjectPointService
{
    Task<List<ProjectPointSummary>> GetForProjectAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPoint?> GetAsync(int projectId, int id, CancellationToken ct = default);

    /// <summary>The number to pre-fill the add form with, so points don't all land on 0.</summary>
    Task<int> GetNextSortOrderAsync(int projectId, CancellationToken ct = default);

    Task<int> CreateAsync(ProjectPoint point, CancellationToken ct = default);

    /// <summary>False when the point doesn't exist on that project.</summary>
    Task<bool> UpdateAsync(ProjectPoint point, CancellationToken ct = default);

    Task<bool> DeleteAsync(int projectId, int id, CancellationToken ct = default);
}
