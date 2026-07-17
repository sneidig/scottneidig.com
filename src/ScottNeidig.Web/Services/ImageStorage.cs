using Microsoft.Extensions.Options;
using ScottNeidig.Web.Configuration;

namespace ScottNeidig.Web.Services;

public class ImageStorage : IImageStorage
{
    /// <summary>Suffix on the mobile variant, per the CodeStitch picture-element convention.</summary>
    public const string MobileSuffix = "-m";

    public const string Extension = ".webp";

    private readonly string _root;
    private readonly ILogger<ImageStorage> _log;

    public ImageStorage(IWebHostEnvironment env, IOptions<ImageOptions> options, ILogger<ImageStorage> log)
    {
        _root = Path.Combine(env.WebRootPath, options.Value.UploadFolder);
        _log = log;
    }

    public async Task SaveAsync(string baseName, ProcessedImage image, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_root);

        await File.WriteAllBytesAsync(PathFor(baseName, desktop: true), image.Desktop, ct);
        await File.WriteAllBytesAsync(PathFor(baseName, desktop: false), image.Mobile, ct);
    }

    public void Delete(string baseName)
    {
        foreach (var path in new[] { PathFor(baseName, true), PathFor(baseName, false) })
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException ex)
            {
                // A locked or read-only file leaves an orphan on disk. That's untidy, not
                // broken, and it must not stop the row being deleted. Logged so it's findable.
                _log.LogWarning(ex, "Could not delete image file {Path}", path);
            }
        }
    }

    public bool Exists(string baseName) =>
        File.Exists(PathFor(baseName, true)) || File.Exists(PathFor(baseName, false));

    /// <summary>
    /// Path.GetFileName strips any directory part, so a base name carrying ../ can't walk
    /// out of the uploads folder. Names are generated, not posted, but this is cheap.
    /// </summary>
    private string PathFor(string baseName, bool desktop)
    {
        var safe = Path.GetFileName(baseName);
        return Path.Combine(_root, $"{safe}{(desktop ? "" : MobileSuffix)}{Extension}");
    }
}
