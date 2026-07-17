using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ScottNeidig.Web.Configuration;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;
using ScottNeidig.Web.Utilities;

namespace ScottNeidig.Web.Services;

public class ProjectImageService : IProjectImageService
{
    private readonly AppDbContext _db;
    private readonly IImageProcessor _processor;
    private readonly IImageStorage _storage;
    private readonly ImageOptions _options;
    private readonly ILogger<ProjectImageService> _log;

    public ProjectImageService(
        AppDbContext db,
        IImageProcessor processor,
        IImageStorage storage,
        IOptions<ImageOptions> options,
        ILogger<ProjectImageService> log)
    {
        _db = db;
        _processor = processor;
        _storage = storage;
        _options = options.Value;
        _log = log;
    }

    public Task<List<ProjectImageSummary>> GetForProjectAsync(int projectId, CancellationToken ct = default) =>
        _db.ProjectImages
            .Where(i => i.ProjectId == projectId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .Select(i => new ProjectImageSummary(i.Id, i.FileName, i.Caption, i.SortOrder, i.IsHero))
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<ProjectImage?> GetAsync(int projectId, int id, CancellationToken ct = default) =>
        _db.ProjectImages.FirstOrDefaultAsync(i => i.Id == id && i.ProjectId == projectId, ct);

    public async Task<int> GetNextSortOrderAsync(int projectId, CancellationToken ct = default)
    {
        var highest = await _db.ProjectImages
            .Where(i => i.ProjectId == projectId)
            .MaxAsync(i => (int?)i.SortOrder, ct);

        return (highest ?? -1) + 1;
    }

    public async Task<UploadResult> UploadAsync(
        int projectId, Stream source, long length, string caption, CancellationToken ct = default)
    {
        if (length <= 0)
        {
            return new UploadResult(UploadFailure.Empty);
        }

        // Checked before decoding, so an oversized file is rejected on its length rather
        // than after SkiaSharp has pulled the whole thing into memory.
        if (length > _options.MaxUploadBytes)
        {
            return new UploadResult(UploadFailure.TooLarge);
        }

        var processed = _processor.Process(source, _options);
        if (processed is null)
        {
            return new UploadResult(UploadFailure.NotAnImage);
        }

        var baseName = GenerateFileName(caption);
        await _storage.SaveAsync(baseName, processed, ct);

        var image = new ProjectImage
        {
            ProjectId = projectId,
            FileName = baseName,
            Caption = caption.Trim(),
            SortOrder = await GetNextSortOrderAsync(projectId, ct),
            // First image on a project becomes the hero, since a project page needs one and
            // forgetting to tick the box is the easy mistake.
            IsHero = !await _db.ProjectImages.AnyAsync(i => i.ProjectId == projectId, ct)
        };

        _db.ProjectImages.Add(image);
        await _db.SaveChangesAsync(ct);

        _log.LogInformation(
            "Uploaded image {FileName} for project {ProjectId} ({DesktopBytes}B desktop, {MobileBytes}B mobile)",
            baseName, projectId, processed.Desktop.Length, processed.Mobile.Length);

        return new UploadResult(UploadFailure.None, image.Id);
    }

    public async Task<bool> UpdateAsync(ProjectImage image, CancellationToken ct = default)
    {
        var existing = await GetAsync(image.ProjectId, image.Id, ct);
        if (existing is null)
        {
            return false;
        }

        // FileName is deliberately not copied: the file on disk is what it is, and letting
        // a form field rewrite it would point the row at a file that doesn't exist.
        existing.Caption = image.Caption;
        existing.SortOrder = image.SortOrder;
        existing.IsHero = image.IsHero;

        if (image.IsHero)
        {
            await ClearOtherHeroesAsync(image.ProjectId, image.Id, ct);
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int projectId, int id, CancellationToken ct = default)
    {
        var image = await GetAsync(projectId, id, ct);
        if (image is null)
        {
            return false;
        }

        _db.ProjectImages.Remove(image);
        await _db.SaveChangesAsync(ct);

        // Files go after the row is safely gone. The other order risks deleting the file
        // and then failing to save, which would leave a row pointing at nothing.
        _storage.Delete(image.FileName);
        return true;
    }

    /// <summary>A project has one hero, so promoting an image demotes whatever held it.</summary>
    private async Task ClearOtherHeroesAsync(int projectId, int keepId, CancellationToken ct)
    {
        var others = await _db.ProjectImages
            .Where(i => i.ProjectId == projectId && i.Id != keepId && i.IsHero)
            .ToListAsync(ct);

        foreach (var other in others)
        {
            other.IsHero = false;
        }
    }

    /// <summary>
    /// Descriptive name from the caption, since the filename is a small image-search signal,
    /// plus a random suffix. The suffix means re-uploading never overwrites a file the
    /// browser has already cached, and two images captioned the same don't collide.
    /// Falls back to "image" when the caption slugs to nothing, e.g. a caption of "3D".
    /// </summary>
    private string GenerateFileName(string caption)
    {
        var slug = SlugGenerator.Generate(caption, maxLength: 60);
        if (string.IsNullOrEmpty(slug))
        {
            slug = "image";
        }

        string name;
        do
        {
            name = $"{slug}-{Random.Shared.Next(0x10000):x4}";
        }
        while (_storage.Exists(name));

        return name;
    }
}
