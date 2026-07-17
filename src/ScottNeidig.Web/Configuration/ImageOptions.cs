namespace ScottNeidig.Web.Configuration;

/// <summary>
/// Targets for the upload pipeline. Config rather than constants because the real display
/// size isn't settled until the public pages exist, and tuning a number in appsettings
/// beats a rebuild. Changing a width only affects new uploads; existing files stay as they
/// were processed.
/// </summary>
public class ImageOptions
{
    public const string SectionName = "Images";

    /// <summary>
    /// Widths are 2x the intended display size, so the image stays sharp on retina screens.
    /// Height follows the source aspect ratio: screenshots get cut off by a fixed crop, and
    /// the part that gets cut is the content.
    /// </summary>
    public int DesktopWidth { get; set; } = 1600;

    public int MobileWidth { get; set; } = 800;

    /// <summary>WEBP quality 0-100. 80 is roughly the knee of the size/quality curve.</summary>
    public int Quality { get; set; } = 80;

    /// <summary>Rejected before anything is decoded, so a huge file can't tie up memory.</summary>
    public long MaxUploadBytes { get; set; } = 8 * 1024 * 1024;

    /// <summary>Folder under wwwroot. Also the public URL segment.</summary>
    public string UploadFolder { get; set; } = "uploads";
}
