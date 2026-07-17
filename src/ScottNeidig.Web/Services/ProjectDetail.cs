namespace ScottNeidig.Web.Services;

/// <summary>
/// Everything /work/{slug} renders, in one round trip.
/// </summary>
public record ProjectDetail(
    string Slug,
    string Title,
    string? Summary,
    string? MetaText,
    string? LiveUrl,
    string? CategoryName,
    string? CategorySlug,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProjectPointSummary> Points,
    IReadOnlyList<ProjectImageSummary> Images)
{
    /// <summary>
    /// The hero is the one image that renders above the fold, so it is never lazy loaded.
    /// Falls back to the first image when nothing is flagged, which can happen if the hero
    /// was deleted. Null only when the project has no images at all.
    /// </summary>
    public ProjectImageSummary? Hero => Images.FirstOrDefault(i => i.IsHero) ?? Images.FirstOrDefault();

    /// <summary>Everything except the hero, in order, for the gallery below the fold.</summary>
    public IEnumerable<ProjectImageSummary> Gallery => Images.Where(i => i.Id != Hero?.Id);

    /// <summary>Falls back to the title, so the tag is never empty.</summary>
    public string PageTitle => string.IsNullOrWhiteSpace(SeoTitle) ? Title : SeoTitle;

    public string? PageDescription => string.IsNullOrWhiteSpace(SeoDescription) ? Summary : SeoDescription;
}
