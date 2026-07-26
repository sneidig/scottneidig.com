using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Models;

/// <summary>
/// A blog post plus its cross-links: when the post's category feeds a service page, the matching
/// projects and a link to that page appear at the bottom. Empty projects means the section hides.
/// </summary>
public class BlogPostViewModel
{
    public required BlogPostDetail Post { get; init; }

    public IReadOnlyList<ProjectCard> RelatedProjects { get; init; } = [];

    /// <summary>The ServicesController action for the matched service page, or null if none.</summary>
    public string? ServiceAction => ServicePages.ActionName(Post.CategoryServiceKey);

    public string? ServiceName => ServicePages.DisplayName(Post.CategoryServiceKey);

    /// <summary>Only show the cross-link block when there's a service and work to point at.</summary>
    public bool HasServiceLink => ServiceAction is not null && RelatedProjects.Count > 0;
}
