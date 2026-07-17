namespace ScottNeidig.Web.Services;

/// <summary>
/// A project as it appears in a list on the public site.
/// HeroFileName is null when a project has no images, so every card template has to cope
/// with a missing picture rather than assume one.
/// </summary>
public record ProjectCard(
    string Slug,
    string Title,
    string? Summary,
    string? MetaText,
    string? CategoryName,
    string? HeroFileName,
    string? HeroCaption);
