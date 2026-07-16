using System.ComponentModel.DataAnnotations;

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
}
