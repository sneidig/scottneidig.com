using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// Edit form for an existing image. The file isn't editable: to change the picture you
/// upload a new one and delete this, which keeps the processed variants and the row in step.
/// </summary>
public class ProjectImageFormModel
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    /// <summary>Not posted. Here so the form can show a preview of what's being edited.</summary>
    public string FileName { get; set; } = "";

    [Required]
    [MaxLength(300)]
    [Display(Name = "Caption")]
    public string Caption { get; set; } = "";

    [Display(Name = "Sort order")]
    public int SortOrder { get; set; }

    [Display(Name = "Hero image")]
    public bool IsHero { get; set; }
}
