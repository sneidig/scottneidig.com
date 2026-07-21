using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Models;

/// <summary>
/// Backing model for a service landing page: the page's own copy lives in its view, this
/// carries the related projects pulled from the category that matches the service.
/// </summary>
public class ServicePageModel
{
    /// <summary>Null when the category slug doesn't match anything, so the section just hides.</summary>
    public string? CategoryName { get; init; }

    public string? CategorySlug { get; init; }

    public IReadOnlyList<ProjectCard> Projects { get; init; } = [];

    public bool HasProjects => Projects.Count > 0;
}
