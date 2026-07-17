using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScottNeidig.Web.Configuration;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// Serves /sitemap.xml and /robots.txt. Both are generated rather than static files so the
/// sitemap tracks the database and the URLs use the right origin per environment.
/// </summary>
public class SeoController : Controller
{
    private readonly ISeoService _seo;
    private readonly SiteOptions _site;

    public SeoController(ISeoService seo, IOptions<SiteOptions> site)
    {
        _seo = seo;
        _site = site.Value;
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var entries = await _seo.GetSitemapEntriesAsync(ct);
        var xml = SitemapXml.Build(entries, Origin());

        return Content(xml, "application/xml", Encoding.UTF8);
    }

    [HttpGet("/robots.txt")]
    public IActionResult Robots()
    {
        // Allow everything except the admin area, and point crawlers at the sitemap.
        var body = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Disallow: /admin")
            .AppendLine()
            .AppendLine($"Sitemap: {Origin()}/sitemap.xml")
            .ToString();

        return Content(body, "text/plain", Encoding.UTF8);
    }

    /// <summary>
    /// Configured BaseUrl wins so a canonical host can be pinned; otherwise derive it from the
    /// request, which is right for local dev.
    /// </summary>
    private string Origin() =>
        !string.IsNullOrWhiteSpace(_site.BaseUrl)
            ? _site.BaseUrl.TrimEnd('/')
            : $"{Request.Scheme}://{Request.Host}";
}
