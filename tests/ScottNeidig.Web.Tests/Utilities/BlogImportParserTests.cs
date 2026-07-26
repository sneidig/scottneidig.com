using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Tests.Utilities;

/// <summary>
/// The import parser turns a generated draft into a post. If it drops a field or leaves a
/// relative image path, the post imports broken, so the parsing rules are worth pinning down.
/// </summary>
public class BlogImportParserTests
{
    private const string Draft = """
        <!--
        ADMIN FIELDS

        Slug:            why-nopcommerce-over-wordpress
        Title:           Why I Reach for nopCommerce Over WordPress
        SeoTitle:        nopCommerce vs WordPress
        SeoDescription:  A working developer's honest take.
        Excerpt:         WordPress runs most of the web, so it is the default.
        Target keyword:  nopCommerce vs WordPress
        -->

        WordPress runs a large share of the web.

        ![A diagram](images/architecture.svg)
        """;

    [Fact]
    public void ParseFields_reads_each_stored_field()
    {
        var fields = BlogImportParser.ParseFields(Draft);

        Assert.Equal("why-nopcommerce-over-wordpress", fields.Slug);
        Assert.Equal("Why I Reach for nopCommerce Over WordPress", fields.Title);
        Assert.Equal("nopCommerce vs WordPress", fields.SeoTitle);
        Assert.Equal("A working developer's honest take.", fields.SeoDescription);
        Assert.Equal("WordPress runs most of the web, so it is the default.", fields.Excerpt);
    }

    [Fact]
    public void ParseFields_returns_nulls_when_there_is_no_front_matter()
    {
        var fields = BlogImportParser.ParseFields("Just a body, no comment block.");

        Assert.Null(fields.Slug);
        Assert.Null(fields.Title);
    }

    [Fact]
    public void StripFrontMatter_removes_the_comment_block()
    {
        var body = BlogImportParser.StripFrontMatter(Draft);

        Assert.DoesNotContain("ADMIN FIELDS", body);
        Assert.DoesNotContain("Target keyword", body);
        Assert.StartsWith("WordPress runs a large share", body);
    }

    [Fact]
    public void RewriteImagePaths_repoints_relative_markdown_images_at_the_post_folder()
    {
        var body = BlogImportParser.RewriteImagePaths(
            "![A diagram](images/architecture.svg)", "my-post");

        Assert.Equal("![A diagram](/uploads/blog/my-post/architecture.svg)", body);
    }

    [Fact]
    public void RewriteImagePaths_repoints_raw_img_tags_too()
    {
        var body = BlogImportParser.RewriteImagePaths(
            """<img src="images/chart.svg" alt="Chart" />""", "my-post");

        Assert.Contains("""src="/uploads/blog/my-post/chart.svg" """, body);
    }

    [Fact]
    public void RewriteImagePaths_leaves_absolute_urls_alone()
    {
        const string body = "![x](https://example.com/images/pic.svg)";

        Assert.Equal(body, BlogImportParser.RewriteImagePaths(body, "my-post"));
    }
}
