using System.Xml.Linq;
using ScottNeidig.Web.Services;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Tests.Utilities;

/// <summary>
/// The sitemap is machine-read by crawlers, so invalid XML or a wrong URL fails silently:
/// nobody sees it, the pages just don't get discovered. Worth pinning down.
/// </summary>
public class SitemapXmlTests
{
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public void Build_produces_parseable_xml_in_the_sitemap_namespace()
    {
        var xml = SitemapXml.Build([new SitemapEntry("/", Priority: 1.0)], "https://scottneidig.com");

        var doc = XDocument.Parse(xml);

        Assert.Equal(Ns + "urlset", doc.Root!.Name);
    }

    [Fact]
    public void Build_joins_origin_and_path_into_an_absolute_loc()
    {
        var xml = SitemapXml.Build([new SitemapEntry("/work/jambalam")], "https://scottneidig.com");

        var loc = XDocument.Parse(xml).Descendants(Ns + "loc").Single().Value;

        Assert.Equal("https://scottneidig.com/work/jambalam", loc);
    }

    [Fact]
    public void Build_does_not_double_the_slash_when_origin_has_a_trailing_one()
    {
        var xml = SitemapXml.Build([new SitemapEntry("/about")], "https://scottneidig.com/");

        var loc = XDocument.Parse(xml).Descendants(Ns + "loc").Single().Value;

        Assert.Equal("https://scottneidig.com/about", loc);
    }

    [Fact]
    public void Build_writes_lastmod_as_a_w3c_date_only_when_present()
    {
        var entries = new[]
        {
            new SitemapEntry("/work/one", new DateTime(2026, 3, 9, 14, 30, 0, DateTimeKind.Utc)),
            new SitemapEntry("/work/two"),
        };

        var doc = XDocument.Parse(SitemapXml.Build(entries, "https://scottneidig.com"));
        var urls = doc.Descendants(Ns + "url").ToList();

        Assert.Equal("2026-03-09", urls[0].Element(Ns + "lastmod")!.Value);
        Assert.Null(urls[1].Element(Ns + "lastmod"));
    }

    [Fact]
    public void Build_formats_priority_with_one_decimal_and_invariant_point()
    {
        var xml = SitemapXml.Build([new SitemapEntry("/", Priority: 1.0)], "https://scottneidig.com");

        // Invariant culture: always a '.', never a ',', regardless of the server's locale.
        Assert.Equal("1.0", XDocument.Parse(xml).Descendants(Ns + "priority").Single().Value);
    }

    [Fact]
    public void Build_escapes_special_characters_in_a_path()
    {
        // A slug shouldn't contain an ampersand, but if one ever reaches here the output must
        // still be valid XML rather than a broken document. XDocument escapes it; this proves it.
        var xml = SitemapXml.Build([new SitemapEntry("/work/a&b")], "https://scottneidig.com");

        // Parses without throwing, and the value round-trips decoded.
        var loc = XDocument.Parse(xml).Descendants(Ns + "loc").Single().Value;
        Assert.Equal("https://scottneidig.com/work/a&b", loc);
    }
}
