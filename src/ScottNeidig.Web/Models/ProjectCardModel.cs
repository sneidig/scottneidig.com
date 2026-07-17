using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Models;

/// <summary>Input for _ProjectCard.cshtml.</summary>
public class ProjectCardModel
{
    public required ProjectCard Project { get; init; }

    /// <summary>
    /// The card's heading level, which depends on what's above it: h2 under the h1 on /work,
    /// h3 on the home page where a section h2 already sits above the grid. Headings are how
    /// screen reader users navigate a page, so skipping a level is a real defect, not a
    /// style question. Only 2 and 3 are supported; anything else renders an h2.
    /// </summary>
    public int HeadingLevel { get; init; } = 2;

    /// <summary>
    /// Shows the byline under the title. On /work it adds context; on the home page it's
    /// noise competing with the summary.
    /// </summary>
    public bool ShowMeta { get; init; }
}
