namespace ScottNeidig.Web.Configuration;

/// <summary>
/// Site-wide values the SEO layer needs: the canonical origin, and the identity used in
/// Open Graph tags and JSON-LD. Not secrets, so they live in appsettings.
/// </summary>
public class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>
    /// Canonical origin, e.g. https://scottneidig.com, no trailing slash. Left empty in the
    /// committed config so dev derives it from the request (localhost) and production sets it.
    /// Set it to pin a canonical host, for instance to force non-www over www.
    /// </summary>
    public string BaseUrl { get; set; } = "";

    public string SiteName { get; set; } = "Scott Neidig";

    public string PersonName { get; set; } = "Scott Neidig";

    public string JobTitle { get; set; } = "Web and application developer";

    // Locality, Region and AreasServed were removed with the local-SEO framing: this site is a
    // portfolio, and location is a contact and territory signal that doesn't belong on one.

    /// <summary>
    /// Cloudflare Web Analytics beacon token. It ships in the page, so it is public, not a
    /// secret, and lives here. Empty by default; the beacon only renders when this is set and
    /// the environment is not Development (see _Analytics.cshtml), so local runs never report.
    /// Get the token from the Cloudflare dashboard under Web Analytics.
    /// </summary>
    public string CloudflareAnalyticsToken { get; set; } = "";
}
