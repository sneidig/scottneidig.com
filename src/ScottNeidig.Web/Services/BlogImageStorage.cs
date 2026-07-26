using Microsoft.Extensions.Options;
using ScottNeidig.Web.Configuration;

namespace ScottNeidig.Web.Services;

public class BlogImageStorage : IBlogImageStorage
{
    private readonly string _root;
    private readonly ILogger<BlogImageStorage> _log;

    public BlogImageStorage(IWebHostEnvironment env, IOptions<ImageOptions> options, ILogger<BlogImageStorage> log)
    {
        // Under the same uploads folder as project images, in a blog/ subtree so the two never
        // collide and the whole thing stays outside the deploy's mirror path.
        _root = Path.Combine(env.WebRootPath, options.Value.UploadFolder, "blog");
        _log = log;
    }

    public async Task SaveAsync(string slug, string fileName, byte[] bytes, CancellationToken ct = default)
    {
        var dir = PostDir(slug);
        Directory.CreateDirectory(dir);

        // GetFileName strips any path in the name, so a zip entry can't write outside the folder.
        var path = Path.Combine(dir, Path.GetFileName(fileName));
        await File.WriteAllBytesAsync(path, bytes, ct);
    }

    public void DeleteAllForPost(string slug)
    {
        var dir = PostDir(slug);
        if (!Directory.Exists(dir))
        {
            return;
        }

        try
        {
            Directory.Delete(dir, recursive: true);
        }
        catch (IOException ex)
        {
            // A locked file leaves stale images behind, which is untidy, not broken. Logged.
            _log.LogWarning(ex, "Could not clear blog image folder for {Slug}", slug);
        }
    }

    /// <summary>GetFileName on the slug too, so a slug can't escape the blog folder.</summary>
    private string PostDir(string slug) => Path.Combine(_root, Path.GetFileName(slug));
}
