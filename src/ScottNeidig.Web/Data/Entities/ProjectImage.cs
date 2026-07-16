using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A screenshot belonging to a project.
/// Stores the base filename only. The picture element builds the desktop and mobile
/// (-m suffix) source paths from it, so the DB never knows about the variants.
/// </summary>
public class ProjectImage
{
    public int Id { get; set; }

    [MaxLength(260)]
    public string FileName { get; set; } = "";

    /// <summary>Also used as the img alt text, so it is not optional.</summary>
    [MaxLength(300)]
    public string Caption { get; set; } = "";

    public int SortOrder { get; set; }

    /// <summary>The hero image is shown large and is never lazy loaded (it is above the fold).</summary>
    public bool IsHero { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
