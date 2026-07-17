using ScottNeidig.Web.Configuration;
using ScottNeidig.Web.Services;
using SkiaSharp;

namespace ScottNeidig.Web.Tests.Services;

/// <summary>
/// The pipeline runs once per upload and its output is what every visitor downloads. A
/// silent regression here means either blurry screenshots or a page that fails PageSpeed,
/// and neither is obvious from looking at the admin.
/// </summary>
public class ImageProcessorTests
{
    private readonly ImageProcessor _processor = new();

    private static readonly ImageOptions Options = new()
    {
        DesktopWidth = 1600,
        MobileWidth = 800,
        Quality = 80
    };

    [Fact]
    public void Process_resizes_to_the_configured_widths()
    {
        using var source = MakeImage(3000, 2000);

        var result = _processor.Process(source, Options);

        Assert.NotNull(result);
        Assert.Equal(1600, WidthOf(result.Desktop));
        Assert.Equal(800, WidthOf(result.Mobile));
    }

    [Fact]
    public void Process_keeps_the_source_aspect_ratio()
    {
        // 3:2 source. A fixed crop would force these to a set shape and cut the rest off.
        using var source = MakeImage(3000, 2000);

        var result = _processor.Process(source, Options);

        Assert.NotNull(result);
        Assert.Equal(1067, HeightOf(result.Desktop));
        Assert.Equal(533, HeightOf(result.Mobile));
    }

    [Fact]
    public void Process_handles_a_portrait_source()
    {
        using var source = MakeImage(1000, 2500);

        var result = _processor.Process(source, Options);

        Assert.NotNull(result);
        Assert.Equal(800, WidthOf(result.Mobile));
        Assert.Equal(2000, HeightOf(result.Mobile));
    }

    [Fact]
    public void Process_does_not_upscale_a_source_smaller_than_the_target()
    {
        // Upscaling invents detail that isn't in the file and only makes it bigger.
        using var source = MakeImage(400, 300);

        var result = _processor.Process(source, Options);

        Assert.NotNull(result);
        Assert.Equal(400, WidthOf(result.Desktop));
        Assert.Equal(400, WidthOf(result.Mobile));
    }

    [Fact]
    public void Process_outputs_webp()
    {
        using var source = MakeImage(1200, 800);

        var result = _processor.Process(source, Options);

        Assert.NotNull(result);
        Assert.Equal(SKEncodedImageFormat.Webp, FormatOf(result.Desktop));
        Assert.Equal(SKEncodedImageFormat.Webp, FormatOf(result.Mobile));
    }

    [Fact]
    public void Process_returns_null_when_the_stream_is_not_an_image()
    {
        // The decode is the only real validation. A content-type header and a file
        // extension are both claims made by whoever is posting.
        using var notAnImage = new MemoryStream("This is a text file pretending to be a png."u8.ToArray());

        Assert.Null(_processor.Process(notAnImage, Options));
    }

    [Fact]
    public void Process_returns_null_on_an_empty_stream()
    {
        using var empty = new MemoryStream();

        Assert.Null(_processor.Process(empty, Options));
    }

    /// <summary>A PNG of the given size, drawn as a gradient so it doesn't compress to nothing.</summary>
    private static MemoryStream MakeImage(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint())
        {
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [SKColors.Coral, SKColors.MidnightBlue],
                SKShaderTileMode.Clamp);

            canvas.DrawRect(0, 0, width, height, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;

        return stream;
    }

    private static SKCodec Decode(byte[] bytes) => SKCodec.Create(new MemoryStream(bytes));

    private static int WidthOf(byte[] bytes)
    {
        using var codec = Decode(bytes);
        return codec.Info.Width;
    }

    private static int HeightOf(byte[] bytes)
    {
        using var codec = Decode(bytes);
        return codec.Info.Height;
    }

    private static SKEncodedImageFormat FormatOf(byte[] bytes)
    {
        using var codec = Decode(bytes);
        return codec.EncodedFormat;
    }
}
