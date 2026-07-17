using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Utilities;

/// <summary>
/// Turns sitemap entries into the XML at /sitemap.xml. A pure function of (entries, origin):
/// no DB, no request, no clock, so it's cheap to test that the output stays valid, which is the
/// point of testing it at all. Built with XDocument so loc values are escaped for free.
/// </summary>
public static class SitemapXml
{
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public static string Build(IEnumerable<SitemapEntry> entries, string origin)
    {
        var baseUrl = origin.TrimEnd('/');

        var urlset = new XElement(Ns + "urlset",
            entries.Select(entry =>
            {
                var url = new XElement(Ns + "url",
                    new XElement(Ns + "loc", baseUrl + entry.Path));

                if (entry.LastModifiedUtc is { } modified)
                {
                    // W3C date; date-only is valid and enough for a brochure site.
                    url.Add(new XElement(Ns + "lastmod", modified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                }

                if (entry.Priority is { } priority)
                {
                    url.Add(new XElement(Ns + "priority", priority.ToString("0.0", CultureInfo.InvariantCulture)));
                }

                return url;
            }));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);

        // StringWriter reports UTF-16, which would put the wrong encoding in the declaration.
        // The subclass forces UTF-8 so the declaration matches the bytes actually served.
        using var writer = new Utf8StringWriter();
        doc.Save(writer);
        return writer.ToString();
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
