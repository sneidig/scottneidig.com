using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>
/// The upload row on the images list. Sort order and hero aren't here: the first upload
/// becomes the hero automatically and sort order lands at the end, both editable after.
/// One less thing to fill in per screenshot.
/// </summary>
public class ProjectImageUploadModel
{
    public int ProjectId { get; set; }

    /// <summary>Doubles as the alt text and seeds the filename, so it's required.</summary>
    [Required]
    [MaxLength(300)]
    public string Caption { get; set; } = "";

    [Required(ErrorMessage = "Choose a file to upload.")]
    [Display(Name = "Image file")]
    public IFormFile? File { get; set; }
}
