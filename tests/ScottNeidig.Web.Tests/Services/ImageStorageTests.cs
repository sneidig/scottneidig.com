using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ScottNeidig.Web.Configuration;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Tests.Services;

/// <summary>
/// The -m suffix isn't cosmetic: the public picture element builds the mobile source path
/// by appending it to the stored name. If storage and markup disagree, every mobile
/// visitor gets a broken image and the desktop one still looks fine in testing.
/// </summary>
public class ImageStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"sn-tests-{Guid.NewGuid():N}");
    private readonly ImageStorage _storage;

    public ImageStorageTests()
    {
        Directory.CreateDirectory(_root);

        _storage = new ImageStorage(
            new StubEnvironment(_root),
            Options.Create(new ImageOptions { UploadFolder = "uploads" }),
            NullLogger<ImageStorage>.Instance);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string UploadPath(string file) => Path.Combine(_root, "uploads", file);

    private static ProcessedImage SomeImage() => new([1, 2, 3], [4, 5]);

    [Fact]
    public async Task SaveAsync_writes_the_desktop_and_mobile_variants()
    {
        await _storage.SaveAsync("checkout-page-a3f9", SomeImage());

        Assert.True(File.Exists(UploadPath("checkout-page-a3f9.webp")));
        Assert.True(File.Exists(UploadPath("checkout-page-a3f9-m.webp")));
    }

    [Fact]
    public async Task SaveAsync_puts_the_mobile_bytes_in_the_mobile_file()
    {
        await _storage.SaveAsync("checkout-page-a3f9", SomeImage());

        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(UploadPath("checkout-page-a3f9.webp")));
        Assert.Equal(new byte[] { 4, 5 }, await File.ReadAllBytesAsync(UploadPath("checkout-page-a3f9-m.webp")));
    }

    [Fact]
    public async Task SaveAsync_creates_the_upload_folder_when_it_is_missing()
    {
        // First upload on a fresh deploy hits exactly this.
        Assert.False(Directory.Exists(Path.Combine(_root, "uploads")));

        await _storage.SaveAsync("first-one-0001", SomeImage());

        Assert.True(File.Exists(UploadPath("first-one-0001.webp")));
    }

    [Fact]
    public async Task Delete_removes_both_variants()
    {
        await _storage.SaveAsync("gone-beef", SomeImage());

        _storage.Delete("gone-beef");

        Assert.False(File.Exists(UploadPath("gone-beef.webp")));
        Assert.False(File.Exists(UploadPath("gone-beef-m.webp")));
    }

    [Fact]
    public void Delete_is_silent_when_the_files_are_already_gone()
    {
        // A row whose files were removed by hand must still be deletable.
        _storage.Delete("never-existed");
    }

    [Fact]
    public async Task Exists_reports_a_taken_name()
    {
        await _storage.SaveAsync("taken-0001", SomeImage());

        Assert.True(_storage.Exists("taken-0001"));
        Assert.False(_storage.Exists("free-0002"));
    }

    [Fact]
    public async Task Delete_cannot_escape_the_upload_folder()
    {
        var outside = Path.Combine(_root, "secrets.webp");
        await File.WriteAllTextAsync(outside, "do not delete me");

        _storage.Delete("../secrets");

        Assert.True(File.Exists(outside));
    }

    private sealed class StubEnvironment : IWebHostEnvironment
    {
        public StubEnvironment(string webRoot) => WebRootPath = webRoot;

        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string EnvironmentName { get; set; } = "Test";
    }
}
