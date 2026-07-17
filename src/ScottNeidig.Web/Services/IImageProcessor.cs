using ScottNeidig.Web.Configuration;

namespace ScottNeidig.Web.Services;

/// <summary>The two WEBP variants of one upload, as bytes. Nothing here knows about disk.</summary>
public record ProcessedImage(byte[] Desktop, byte[] Mobile);

public interface IImageProcessor
{
    /// <summary>
    /// Decodes the upload and re-encodes it at the desktop and mobile widths.
    /// Returns null when the stream isn't an image the decoder recognises. That decode is
    /// the real validation: a content-type header and a file extension are both just claims
    /// made by whoever is posting.
    /// </summary>
    ProcessedImage? Process(Stream source, ImageOptions options);
}
