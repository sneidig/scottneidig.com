using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Models;

/// <summary>Input for _PostCard.cshtml.</summary>
public class PostCardModel
{
    public required BlogListItem Post { get; init; }

    /// <summary>
    /// The card's heading level, which depends on what's above it: h2 under the h1 on /blog,
    /// h3 on the home page where a section h2 already sits above the grid. Same reasoning as
    /// ProjectCardModel.HeadingLevel. Only 2 and 3 are supported; anything else renders an h2.
    /// </summary>
    public int HeadingLevel { get; init; } = 2;
}
