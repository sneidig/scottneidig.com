using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Models;

public class HomeViewModel
{
    /// <summary>
    /// The first few published projects in sort order. Sort order doubles as "featured"
    /// rather than carrying a separate flag: with this many projects it would be a knob
    /// set once, and the admin list already shows the order.
    /// </summary>
    public IReadOnlyList<ProjectCard> FeaturedProjects { get; init; } = [];
}
