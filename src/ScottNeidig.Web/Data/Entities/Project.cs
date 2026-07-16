using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A portfolio item. Gets its own page at /work/{slug}, which is the SEO point:
/// one indexable URL per project rather than a single scrolling page.
/// Most fields are optional so a sparse project still renders.
/// </summary>
public class Project
{
    public int Id { get; set; }

    /// <summary>URL segment. Unique index configured in AppDbContext.</summary>
    [MaxLength(100)]
    public string Slug { get; set; } = "";

    [MaxLength(200)]
    public string Title { get; set; } = "";

    /// <summary>Lead paragraph describing the project.</summary>
    [MaxLength(2000)]
    public string? Summary { get; set; }

    /// <summary>Short byline, e.g. "Built and operated solo".</summary>
    [MaxLength(200)]
    public string? MetaText { get; set; }

    [MaxLength(500)]
    public string? LiveUrl { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Unpublished projects are hidden from the public site but stay editable.</summary>
    public bool Published { get; set; }

    /// <summary>Overrides the page title tag. Falls back to Title when empty.</summary>
    [MaxLength(70)]
    public string? SeoTitle { get; set; }

    [MaxLength(200)]
    public string? SeoDescription { get; set; }

    public DateTime CreatedUtc { get; set; }

    public List<ProjectImage> Images { get; set; } = [];
    public List<ProjectPoint> Points { get; set; } = [];
}
