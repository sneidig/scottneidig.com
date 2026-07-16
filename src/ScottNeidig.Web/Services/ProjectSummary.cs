namespace ScottNeidig.Web.Services;

/// <summary>
/// A project row for the admin list, projected in one query.
/// DTO rather than entity, so it's safe to hand straight to a view.
/// </summary>
public record ProjectSummary(
    int Id,
    string Title,
    string Slug,
    string? CategoryName,
    int SortOrder,
    bool Published,
    int ImageCount,
    int PointCount);
