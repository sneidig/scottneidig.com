namespace ScottNeidig.Web.Services;

/// <summary>
/// One URL in the sitemap. Path is site-relative (e.g. "/work/jambalam"); the absolute URL is
/// built by prepending the site origin at render time, so the entries stay origin-agnostic and
/// testable without a request.
/// </summary>
public record SitemapEntry(string Path, DateTime? LastModifiedUtc = null, double? Priority = null);
