using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Models;

/// <summary>
/// /blog and /blog/category/{slug} share this. Same shape as WorkListViewModel: the posts, the
/// topics to filter by, and which topic is selected (null on the unfiltered list).
/// </summary>
public class BlogListViewModel
{
    public IReadOnlyList<BlogListItem> Posts { get; init; } = [];

    public IReadOnlyList<CategorySummary> Topics { get; init; } = [];

    public CategorySummary? SelectedTopic { get; init; }

    public bool IsFiltered => SelectedTopic is not null;

    /// <summary>"list" (default, full-width rows) or "grid" (cards). Set from the ?view param.</summary>
    public string Layout { get; init; } = "list";

    public bool IsGrid => Layout == "grid";
}
