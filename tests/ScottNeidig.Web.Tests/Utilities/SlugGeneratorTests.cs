using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Tests.Utilities;

/// <summary>
/// Slugs are URLs. If this breaks, pages change address or collide, so it's worth
/// pinning the behaviour down rather than trusting it by eye.
/// </summary>
public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Small Business / SEO", "small-business-seo")]
    [InlineData("nopCommerce", "nopcommerce")]
    [InlineData("Project 2026", "project-2026")]
    public void Generate_converts_titles_to_lowercase_hyphenated_slugs(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Generate(input));
    }

    [Theory]
    [InlineData("C# & .NET", "c-net")]
    [InlineData("Why nopCommerce, not WordPress?", "why-nopcommerce-not-wordpress")]
    [InlineData("a---b", "a-b")]
    [InlineData("Lots   of    spaces", "lots-of-spaces")]
    public void Generate_collapses_runs_of_punctuation_and_whitespace(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Generate(input));
    }

    [Theory]
    [InlineData("  Trimmed  ", "trimmed")]
    [InlineData("---leading and trailing---", "leading-and-trailing")]
    [InlineData("!!!", "")]
    public void Generate_trims_separators_from_the_ends(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Generate(input));
    }

    [Theory]
    [InlineData("Café Münster", "cafe-munster")]
    [InlineData("Jalapeño", "jalapeno")]
    public void Generate_strips_accents_rather_than_dropping_the_letter(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Generate(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Generate_returns_empty_for_no_input(string? input)
    {
        Assert.Equal(string.Empty, SlugGenerator.Generate(input));
    }

    [Fact]
    public void Generate_respects_the_max_length()
    {
        var slug = SlugGenerator.Generate(new string('a', 200), maxLength: 100);

        Assert.Equal(100, slug.Length);
    }

    [Fact]
    public void Generate_does_not_leave_a_trailing_hyphen_when_truncating()
    {
        // Cutting at 12 chars lands mid-separator, which would otherwise leave "aaaa-bbbb-".
        var slug = SlugGenerator.Generate("aaaa bbbb cccc", maxLength: 10);

        Assert.Equal("aaaa-bbbb", slug);
    }

    [Fact]
    public void Generate_is_stable_for_the_same_input()
    {
        const string title = "Some Project Title";

        Assert.Equal(SlugGenerator.Generate(title), SlugGenerator.Generate(title));
    }
}
