namespace ScottNeidig.Web.Services;

public interface ISeoService
{
    /// <summary>
    /// Every public URL for the sitemap: the static pages plus a row per published project and
    /// per non-empty category. Unpublished work is left out, same as the public queries, so the
    /// sitemap never points a crawler at a page that returns 404.
    /// </summary>
    Task<IReadOnlyList<SitemapEntry>> GetSitemapEntriesAsync(CancellationToken ct = default);
}
