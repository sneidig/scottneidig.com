namespace ScottNeidig.Web.Models;

/// <summary>
/// Input for _Picture.cshtml. Built from a stored base filename; the partial appends the
/// -m suffix and .webp to get the two variants the upload pipeline wrote.
/// </summary>
public class PictureModel
{
    /// <summary>Base filename with no suffix and no extension, as stored on ProjectImage.</summary>
    public required string FileName { get; init; }

    /// <summary>Alt text. Empty string is allowed and means decorative; null is not.</summary>
    public required string Alt { get; init; }

    /// <summary>
    /// Above-the-fold images must not be lazy loaded: the browser defers the request, so the
    /// image lands late and shoves the layout around. That's a CLS hit and a slower LCP,
    /// which is the opposite of the point.
    /// </summary>
    public bool AboveTheFold { get; init; }

    /// <summary>
    /// Intrinsic size of the desktop variant, used for the width/height attributes that
    /// reserve layout space. Defaults match ImageOptions; a wrong ratio here costs nothing
    /// visually because the CSS sets height:auto, it just reserves the wrong box briefly.
    /// </summary>
    public int Width { get; init; } = 1600;

    public int Height { get; init; } = 1000;

    public string? CssClass { get; init; }
}
