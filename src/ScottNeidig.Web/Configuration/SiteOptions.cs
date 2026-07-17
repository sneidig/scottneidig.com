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

    // Local SEO. City-level only, no street address, so a home address isn't published. These
    // feed the JSON-LD (addressLocality/addressRegion + areaServed) and the footer.
    public string Locality { get; set; } = "Boulder";

    public string Region { get; set; } = "CO";

    /// <summary>Places the local services are offered, for schema areaServed and the footer.</summary>
    public string[] AreasServed { get; set; } = ["Boulder", "Denver metro", "Colorado Front Range"];
}
