using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

/// <summary>Why an upload was rejected. The controller turns this into a field message.</summary>
public enum UploadFailure
{
    None,
    Empty,
    TooLarge,
    NotAnImage
}

public record UploadResult(UploadFailure Failure, int ImageId = 0)
{
    public bool Succeeded => Failure == UploadFailure.None;
}

/// <summary>
/// Like points, images always belong to a project, so every method takes the project id
/// and scopes its query by it.
/// </summary>
public interface IProjectImageService
{
    Task<List<ProjectImageSummary>> GetForProjectAsync(int projectId, CancellationToken ct = default);

    Task<ProjectImage?> GetAsync(int projectId, int id, CancellationToken ct = default);

    Task<int> GetNextSortOrderAsync(int projectId, CancellationToken ct = default);

    /// <summary>
    /// Processes the upload, writes both variants and saves the row. The stream is read
    /// here rather than in the controller so the controller never touches SkiaSharp.
    /// </summary>
    Task<UploadResult> UploadAsync(int projectId, Stream source, long length, string caption, CancellationToken ct = default);

    /// <summary>Caption, sort order and hero flag only. The file itself is replaced by re-uploading.</summary>
    Task<bool> UpdateAsync(ProjectImage image, CancellationToken ct = default);

    /// <summary>Removes the row and its files.</summary>
    Task<bool> DeleteAsync(int projectId, int id, CancellationToken ct = default);
}
