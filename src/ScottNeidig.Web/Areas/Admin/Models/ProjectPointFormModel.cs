using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// Create/edit form for one project point. Shared by the add row on the list page and
/// the edit page, so the two can't drift apart on validation rules.
/// </summary>
public class ProjectPointFormModel
{
    public int? Id { get; set; }

    /// <summary>Comes from the route, not the form. Here so views can build URLs.</summary>
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    /// <summary>Plain text, rendered with line breaks. Not markdown: a bullet doesn't need it.</summary>
    [MaxLength(1000)]
    [DataType(DataType.MultilineText)]
    public string? Body { get; set; }

    [Display(Name = "Sort order")]
    public int SortOrder { get; set; }
}
