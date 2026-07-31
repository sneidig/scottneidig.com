using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public interface IBlogService
{
    /// <summary>Every post, published or not, for the admin list. Newest first.</summary>
    Task<List<BlogPostSummary>> GetAllAsync(CancellationToken ct = default);

    Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tracked lookup by slug for the admin, used by the importer to update in place.</summary>
    Task<BlogPost?> GetBySlugForEditAsync(string slug, CancellationToken ct = default);

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
    /// <param name="categorySlug">Null for all posts, otherwise limits to that category.</param>
    /// <param name="take">Null for every post, otherwise the newest N (the home page shows a few).</param>
    Task<List<BlogListItem>> GetPublishedAsync(
        string? categorySlug = null, int? take = null, CancellationToken ct = default);

    /// <summary>Categories that have published posts, for the blog filter nav.</summary>
    Task<List<CategorySummary>> GetTopicsAsync(CancellationToken ct = default);

    /// <summary>Null when the slug is unknown or the post isn't published. The caller 404s.</summary>
    Task<BlogPostDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Published posts in a category, newest first, excluding one slug (the post you're already
    /// reading). Powers the related-writing section on a service page and cross-links.
    /// </summary>
    Task<List<BlogListItem>> GetPublishedByCategoryAsync(
        string categorySlug, int take, string? excludeSlug = null, CancellationToken ct = default);
}
