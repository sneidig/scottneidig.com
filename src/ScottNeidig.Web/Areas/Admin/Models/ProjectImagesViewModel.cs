using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Models;

/// <summary>The image list plus the upload row underneath it, same shape as the points page.</summary>
public class ProjectImagesViewModel
{
    public int ProjectId { get; init; }

    public string ProjectTitle { get; init; } = "";

    public List<ProjectImageSummary> Images { get; init; } = [];

    public ProjectImageUploadModel NewImage { get; set; } = new();
}
