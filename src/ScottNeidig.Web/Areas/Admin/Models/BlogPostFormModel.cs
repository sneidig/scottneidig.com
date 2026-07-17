using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// Create/edit form for a blog post. Like projects, the slug is editable and doesn't follow the
/// title on rename, since /blog/{slug} is an indexed URL.
/// </summary>
public class BlogPostFormModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [MaxLength(100)]
    [Display(Name = "URL slug")]
    public string? Slug { get; set; }

    [Display(Name = "Body (Markdown)")]
    [DataType(DataType.MultilineText)]
    public string MarkdownBody { get; set; } = "";

    [MaxLength(500)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Excerpt")]
    public string? Excerpt { get; set; }

    /// <summary>Drafts stay editable but are hidden from the public site.</summary>
    public bool Published { get; set; }

    [MaxLength(70)]
    [Display(Name = "SEO title")]
    public string? SeoTitle { get; set; }

    [MaxLength(200)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "SEO description")]
    public string? SeoDescription { get; set; }
}
