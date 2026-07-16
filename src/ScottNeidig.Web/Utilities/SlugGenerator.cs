using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ScottNeidig.Web.Utilities;

/// <summary>
/// Turns a title into a URL-safe slug. Shared by categories, projects and blog posts.
/// Pure function with no dependencies, which is why it's a static helper rather than
/// a service: nothing to inject and nothing to mock.
/// </summary>
public static partial class SlugGenerator
{
    /// <summary>Anything that isn't a lowercase letter or digit becomes a separator.</summary>
    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugChars();

    /// <summary>
    /// Slugs end up in URLs, so this has to be predictable and stable. Same input always
    /// gives the same output.
    /// </summary>
    /// <param name="input">The source text, usually a title.</param>
    /// <param name="maxLength">Cap matching the Slug column length in the database.</param>
    public static string Generate(string? input, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Split accented characters into base letter + accent mark, then drop the marks,
        // so "Café" becomes "cafe" rather than losing the character entirely.
        var decomposed = input.Normalize(NormalizationForm.FormD);
        var stripped = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stripped.Append(c);
            }
        }

        var slug = stripped
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        // Collapses runs of punctuation and whitespace into a single hyphen, so
        // "C# & .NET" becomes "c-net" rather than "c-----net".
        slug = NonSlugChars().Replace(slug, "-").Trim('-');

        if (slug.Length > maxLength)
        {
            // Trim again in case the cut landed on a hyphen and left a trailing one.
            slug = slug[..maxLength].Trim('-');
        }

        return slug;
    }
}
