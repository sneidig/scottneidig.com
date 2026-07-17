namespace ScottNeidig.Web.Services;

/// <summary>
/// An image row for the admin list.
/// DTO rather than entity, so it's safe to hand straight to a view.
/// </summary>
public record ProjectImageSummary(
    int Id,
    string FileName,
    string Caption,
    int SortOrder,
    bool IsHero);
