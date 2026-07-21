using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// Groups projects so the work list can be filtered (e.g. nopCommerce, Small Business / SEO).
/// </summary>
public class Category
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    /// <summary>URL segment. Unique index configured in AppDbContext.</summary>
    [MaxLength(100)]
    public string Slug { get; set; } = "";

    public int SortOrder { get; set; }

    /// <summary>
    /// Which service landing page this category feeds, or null for none. Values come from
    /// ServicePages. A unique index (filtered to non-nulls) stops two categories claiming the
    /// same page, since each page shows exactly one category's work.
    /// </summary>
    [MaxLength(50)]
    public string? ServiceKey { get; set; }

    public List<Project> Projects { get; set; } = [];
}
