namespace ScottNeidig.Web.Services;

/// <summary>A post row for the admin list.</summary>
public record BlogPostSummary(
    int Id,
    string Title,
    string Slug,
    bool Published,
    DateTime? PublishedUtc);

/// <summary>A post as it appears in the public /blog list.</summary>
public record BlogListItem(
    string Slug,
    string Title,
    string? Excerpt,
    DateTime? PublishedUtc);

/// <summary>
/// A rendered post for /blog/{slug}. BodyHtml is already rendered from markdown, so the view
/// only outputs it and the markdown pipeline stays out of the view.
/// </summary>
public record BlogPostDetail(
    string Slug,
    string Title,
    string BodyHtml,
    DateTime? PublishedUtc,
    string? SeoTitle,
    string? SeoDescription,
    string? Excerpt)
{
    public string PageTitle => string.IsNullOrWhiteSpace(SeoTitle) ? Title : SeoTitle;

    public string? PageDescription => string.IsNullOrWhiteSpace(SeoDescription) ? Excerpt : SeoDescription;
}
