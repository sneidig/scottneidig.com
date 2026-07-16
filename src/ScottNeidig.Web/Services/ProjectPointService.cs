using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class ProjectPointService : IProjectPointService
{
    private readonly AppDbContext _db;

    public ProjectPointService(AppDbContext db) => _db = db;

    public Task<List<ProjectPointSummary>> GetForProjectAsync(int projectId, CancellationToken ct = default) =>
        _db.ProjectPoints
            .Where(p => p.ProjectId == projectId)
            // Id breaks ties, so two points on the same SortOrder keep the order they
            // were entered in instead of whatever the database feels like returning.
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => new ProjectPointSummary(p.Id, p.Title, p.Body, p.SortOrder))
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>Tracked, because the caller edits and saves it.</summary>
    public Task<ProjectPoint?> GetAsync(int projectId, int id, CancellationToken ct = default) =>
        _db.ProjectPoints.FirstOrDefaultAsync(p => p.Id == id && p.ProjectId == projectId, ct);

    public async Task<int> GetNextSortOrderAsync(int projectId, CancellationToken ct = default)
    {
        // Max() over an empty set throws on a non-nullable int, so project to int? first.
        var highest = await _db.ProjectPoints
            .Where(p => p.ProjectId == projectId)
            .MaxAsync(p => (int?)p.SortOrder, ct);

        return (highest ?? -1) + 1;
    }

    public async Task<int> CreateAsync(ProjectPoint point, CancellationToken ct = default)
    {
        _db.ProjectPoints.Add(point);
        await _db.SaveChangesAsync(ct);

        return point.Id;
    }

    public async Task<bool> UpdateAsync(ProjectPoint point, CancellationToken ct = default)
    {
        var existing = await GetAsync(point.ProjectId, point.Id, ct);
        if (existing is null)
        {
            return false;
        }

        // Field by field for the same reason as ProjectService.UpdateAsync: attaching the
        // posted instance would let it reassign ProjectId and move the point to another project.
        existing.Title = point.Title;
        existing.Body = point.Body;
        existing.SortOrder = point.SortOrder;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int projectId, int id, CancellationToken ct = default)
    {
        var point = await GetAsync(projectId, id, ct);
        if (point is null)
        {
            return false;
        }

        _db.ProjectPoints.Remove(point);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
