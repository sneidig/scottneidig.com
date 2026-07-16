using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// The points list plus the add row underneath it, on one page. Adding several bullets in a
/// row is the normal case, so the add form stays on the list instead of being its own page.
/// </summary>
public class ProjectPointsViewModel
{
    public int ProjectId { get; init; }

    public string ProjectTitle { get; init; } = "";

    public List<ProjectPointSummary> Points { get; init; } = [];

    public ProjectPointFormModel NewPoint { get; set; } = new();
}
