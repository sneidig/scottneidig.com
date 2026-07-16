using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A heading for a set of skills, e.g. "Backend", "Frontend".
/// </summary>
public class SkillGroup
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Label { get; set; } = "";

    public int SortOrder { get; set; }

    public List<Skill> Skills { get; set; } = [];
}
