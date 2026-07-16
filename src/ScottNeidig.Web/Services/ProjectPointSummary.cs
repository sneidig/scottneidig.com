namespace ScottNeidig.Web.Services;

/// <summary>
/// A point row for the admin list.
/// DTO rather than entity, so it's safe to hand straight to a view.
/// </summary>
public record ProjectPointSummary(
    int Id,
    string Title,
    string Body,
    int SortOrder);
