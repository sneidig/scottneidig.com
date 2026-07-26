namespace ScottNeidig.Web.Services;

/// <summary>
/// Stores a blog post's images under wwwroot/uploads/blog/{slug}/. Separate from the project
/// image pipeline: these are SVG diagrams and the like, served straight as files, not photos
/// pushed through SkiaSharp.
/// </summary>
public interface IBlogImageStorage
{
    /// <summary>Writes one image into the post's folder, creating it if needed.</summary>
    Task SaveAsync(string slug, string fileName, byte[] bytes, CancellationToken ct = default);

    /// <summary>
    /// Clears the post's folder before a re-import, so a renamed or removed image doesn't linger
    /// and get served after it's gone from the draft.
    /// </summary>
    void DeleteAllForPost(string slug);
}
