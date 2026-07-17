namespace ScottNeidig.Web.Services;

/// <summary>
/// Reads and writes the variant files under wwwroot. Separate from the processor so the
/// processor stays testable without a disk, and separate from the services so both the
/// image service and project deletion can clean up files through one path.
/// </summary>
public interface IImageStorage
{
    /// <summary>Writes baseName.webp and baseName-m.webp. Overwrites if they somehow exist.</summary>
    Task SaveAsync(string baseName, ProcessedImage image, CancellationToken ct = default);

    /// <summary>
    /// Deletes both variants. Missing files are not an error: the DB row is the record we
    /// care about, and a half-deleted pair shouldn't block removing it.
    /// </summary>
    void Delete(string baseName);

    /// <summary>True when a file with this base name already exists, either variant.</summary>
    bool Exists(string baseName);
}
