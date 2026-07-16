using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// Create/edit form for a project.
/// Unlike categories, the slug is editable and does not follow the title. /work/{slug}
/// is an indexed URL, so silently changing it on a rename would break inbound links.
/// </summary>
public class ProjectFormModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    /// <summary>Left blank on create, it is generated from the title.</summary>
    [MaxLength(100)]
    [Display(Name = "URL slug")]
    public string? Slug { get; set; }

    [MaxLength(2000)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Summary")]
    public string? Summary { get; set; }

    [MaxLength(200)]
    [Display(Name = "Byline")]
    public string? MetaText { get; set; }

    [MaxLength(500)]
    [Url]
    [Display(Name = "Live URL")]
    public string? LiveUrl { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Sort order")]
    public int SortOrder { get; set; }

    /// <summary>Unpublished projects stay editable but are hidden from the public site.</summary>
    public bool Published { get; set; }

    [MaxLength(70)]
    [Display(Name = "SEO title")]
    public string? SeoTitle { get; set; }

    [MaxLength(200)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "SEO description")]
    public string? SeoDescription { get; set; }

    /// <summary>Populated by the controller. Not posted back.</summary>
    public List<SelectListItem> Categories { get; set; } = [];
}
