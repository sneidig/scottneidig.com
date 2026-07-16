using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A blog post. Body is stored as markdown and rendered to HTML on display,
/// so the source stays portable and editable outside the app.
/// </summary>
public class BlogPost
{
    public int Id { get; set; }

    /// <summary>URL segment. Unique index configured in AppDbContext.</summary>
    [MaxLength(100)]
    public string Slug { get; set; } = "";

    [MaxLength(200)]
    public string Title { get; set; } = "";

    /// <summary>Markdown source. No length cap: post bodies are legitimately long.</summary>
    public string MarkdownBody { get; set; } = "";

    /// <summary>Shown on the post list. Also the fallback meta description.</summary>
    [MaxLength(500)]
    public string? Excerpt { get; set; }

    /// <summary>Null until published.</summary>
    public DateTime? PublishedUtc { get; set; }

    public bool Published { get; set; }

    [MaxLength(70)]
    public string? SeoTitle { get; set; }

    [MaxLength(200)]
    public string? SeoDescription { get; set; }
}
