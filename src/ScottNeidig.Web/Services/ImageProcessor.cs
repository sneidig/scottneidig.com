using ScottNeidig.Web.Configuration;
using SkiaSharp;

namespace ScottNeidig.Web.Services;

/// <summary>
/// SkiaSharp rather than ImageSharp: ImageSharp went to a licence that costs money for
/// commercial use, and this pipeline is meant to lift into paid client sites. SkiaSharp
/// is MIT, so it comes along for free.
/// </summary>
public class ImageProcessor : IImageProcessor
{
    public ProcessedImage? Process(Stream source, ImageOptions options)
    {
        // Decode returns null on anything that isn't a supported image, including a .png
        // that is really a zip. It doesn't throw, so there's nothing to catch here.
        using var original = SKBitmap.Decode(source);
        if (original is null || original.Width == 0 || original.Height == 0)
        {
            return null;
        }

        var desktop = Encode(original, options.DesktopWidth, options.Quality);
        var mobile = Encode(original, options.MobileWidth, options.Quality);

        return desktop is null || mobile is null ? null : new ProcessedImage(desktop, mobile);
    }

    private static byte[]? Encode(SKBitmap original, int targetWidth, int quality)
    {
        using var sized = Resize(original, targetWidth);
        if (sized is null)
        {
            return null;
        }

        using var image = SKImage.FromBitmap(sized);
        using var data = image.Encode(SKEncodedImageFormat.Webp, quality);

        return data?.ToArray();
    }

    /// <summary>
    /// Scales to the target width and lets the height follow, so the aspect ratio is kept
    /// and nothing is cropped away. A source narrower than the target is returned untouched:
    /// upscaling invents detail that isn't there and only makes the file bigger.
    /// </summary>
    private static SKBitmap? Resize(SKBitmap original, int targetWidth)
    {
        if (original.Width <= targetWidth)
        {
            return original.Copy();
        }

        // Round rather than truncate, so a 1000x667 source doesn't lose a row to flooring.
        var targetHeight = (int)Math.Round(original.Height * (double)targetWidth / original.Width);
        var info = new SKImageInfo(targetWidth, Math.Max(1, targetHeight));

        // Mitchell is the usual pick for downscaling photos and screenshots: sharper than
        // linear, without the ringing that Catmull-Rom puts on hard UI edges.
        return original.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell));
    }
}
