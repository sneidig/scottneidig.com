using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A single skill inside a SkillGroup.
/// </summary>
public class Skill
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    public int SortOrder { get; set; }

    public int SkillGroupId { get; set; }
    public SkillGroup? SkillGroup { get; set; }
}
