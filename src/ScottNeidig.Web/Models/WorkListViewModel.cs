using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Models;

/// <summary>
/// /work and /work/category/{slug} render the same page. The only difference is whether a
/// category is selected, so they share a view rather than duplicating the grid.
/// </summary>
public class WorkListViewModel
{
    public IReadOnlyList<ProjectCard> Projects { get; init; } = [];

    public IReadOnlyList<CategorySummary> Categories { get; init; } = [];

    /// <summary>Null on the unfiltered /work.</summary>
    public CategorySummary? SelectedCategory { get; init; }

    public bool IsFiltered => SelectedCategory is not null;
}
