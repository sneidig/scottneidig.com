using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public interface IBlogService
{
    /// <summary>Every post, published or not, for the admin list. Newest first.</summary>
    Task<List<BlogPostSummary>> GetAllAsync(CancellationToken ct = default);

    Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(BlogPost post, CancellationToken ct = default);

    /// <summary>False when the post no longer exists.</summary>
    Task<bool> UpdateAsync(BlogPost post, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>True when another post already uses this slug. Checked before saving.</summary>
    Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default);

    /// <summary>
    /// Published posts only, newest first, for the public list. Kept separate from GetAllAsync
    /// so a draft can't leak onto a public page through a shared query.
    /// </summary>
    Task<List<BlogListItem>> GetPublishedAsync(CancellationToken ct = default);

    /// <summary>Null when the slug is unknown or the post isn't published. The caller 404s.</summary>
    Task<BlogPostDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);
}
