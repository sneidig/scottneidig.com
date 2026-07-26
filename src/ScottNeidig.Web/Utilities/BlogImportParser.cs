using System.Text.RegularExpressions;

namespace ScottNeidig.Web.Utilities;

/// <summary>
/// Parses a generated blog draft: the leading HTML-comment front-matter block, the body after
/// it, and the image path rewrite. Pure and string-only, so it's testable without a zip or a
/// database. The generator's format is a fixed convention, which is what makes this reliable.
/// </summary>
public static partial class BlogImportParser
{
    public record Fields(
        string? Slug,
        string? Title,
        string? SeoTitle,
        string? SeoDescription,
        string? Excerpt);

    [GeneratedRegex(@"^\s*<!--(.*?)-->", RegexOptions.Singleline)]
    private static partial Regex FrontMatterBlock();

    [GeneratedRegex(@"\]\(\s*images/([^)\s]+)\s*\)")]
    private static partial Regex MarkdownImagePath();

    [GeneratedRegex(@"src\s*=\s*""images/([^""]+)""")]
    private static partial Regex HtmlImagePath();

    /// <summary>
    /// Reads the five stored fields from the leading comment block. A missing block or key comes
    /// back null, so the caller can validate rather than crash on a malformed draft.
    /// </summary>
    public static Fields ParseFields(string markdown)
    {
        var block = FrontMatterBlock().Match(markdown);
        var text = block.Success ? block.Groups[1].Value : "";

        return new Fields(
            Value(text, "Slug"),
            Value(text, "Title"),
            Value(text, "SeoTitle"),
            Value(text, "SeoDescription"),
            Value(text, "Excerpt"));
    }

    /// <summary>The body with the front-matter comment removed, so it doesn't render.</summary>
    public static string StripFrontMatter(string markdown) =>
        FrontMatterBlock().Replace(markdown, "").TrimStart();

    /// <summary>
    /// Repoints the draft's relative image paths (images/foo.svg) at where the importer actually
    /// stores them (/uploads/blog/{slug}/foo.svg). Covers both markdown and raw img tags.
    /// </summary>
    public static string RewriteImagePaths(string body, string slug)
    {
        var target = $"/uploads/blog/{slug}/";

        body = MarkdownImagePath().Replace(body, m => $"]({target}{m.Groups[1].Value})");
        body = HtmlImagePath().Replace(body, m => $"src=\"{target}{m.Groups[1].Value}\"");

        return body;
    }

    /// <summary>Matches "Key: value" on its own line, value being the rest of that line.</summary>
    private static string? Value(string block, string key)
    {
        var match = Regex.Match(block, $@"^\s*{Regex.Escape(key)}:\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
