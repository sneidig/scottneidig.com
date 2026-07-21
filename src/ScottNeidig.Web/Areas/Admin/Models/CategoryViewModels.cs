using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// Create/edit form. Slug isn't editable: it's derived from the name, so there's one
/// source of truth and no way to save a name and slug that disagree.
/// </summary>
public class CategoryFormModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Display(Name = "Sort order")]
    public int SortOrder { get; set; }

    /// <summary>Shown read-only on the edit form so the resulting URL is visible.</summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Which service page shows this category's work. Blank means none. Only one category can
    /// feed each page, enforced by a unique index.
    /// </summary>
    [Display(Name = "Feeds service page")]
    public string? ServiceKey { get; set; }

    /// <summary>Populated by the controller. Not posted back.</summary>
    public List<SelectListItem> ServiceOptions { get; set; } = [];
}
