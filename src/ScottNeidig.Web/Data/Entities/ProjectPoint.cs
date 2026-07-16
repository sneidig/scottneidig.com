using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A bullet point on a project page describing what was done.
/// </summary>
public class ProjectPoint
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    public string Body { get; set; } = "";

    public int SortOrder { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
