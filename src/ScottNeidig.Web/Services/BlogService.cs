using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class BlogService : IBlogService
{
    private readonly AppDbContext _db;
    private readonly IMarkdownRenderer _markdown;

    public BlogService(AppDbContext db, IMarkdownRenderer markdown)
    {
        _db = db;
        _markdown = markdown;
    }

    public Task<List<BlogPostSummary>> GetAllAsync(CancellationToken ct = default) =>
        _db.BlogPosts
            .OrderByDescending(p => p.PublishedUtc ?? DateTime.MaxValue)
            .ThenByDescending(p => p.Id)
            .Select(p => new BlogPostSummary(p.Id, p.Title, p.Slug, p.Published, p.PublishedUtc))
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>Tracked, because the caller edits and saves it.</summary>
    public Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<int> CreateAsync(BlogPost post, CancellationToken ct = default)
    {
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync(ct);

        return post.Id;
    }

    public async Task<bool> UpdateAsync(BlogPost post, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(post.Id, ct);
        if (existing is null)
        {
            return false;
        }

        // Copied field by field so the collection-free entity can't reset anything unmanaged,
        // same reason as ProjectService.UpdateAsync.
        existing.Slug = post.Slug;
        existing.Title = post.Title;
        existing.MarkdownBody = post.MarkdownBody;
        existing.Excerpt = post.Excerpt;
        existing.Published = post.Published;
        existing.PublishedUtc = post.PublishedUtc;
        existing.SeoTitle = post.SeoTitle;
        existing.SeoDescription = post.SeoDescription;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var post = await GetByIdAsync(id, ct);
        if (post is null)
        {
            return false;
        }

        _db.BlogPosts.Remove(post);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludingId = null, CancellationToken ct = default) =>
        _db.BlogPosts.AnyAsync(p => p.Slug == slug && (excludingId == null || p.Id != excludingId), ct);

    public Task<List<BlogListItem>> GetPublishedAsync(CancellationToken ct = default) =>
        _db.BlogPosts
            .Where(p => p.Published)
            .OrderByDescending(p => p.PublishedUtc)
            .ThenByDescending(p => p.Id)
            .Select(p => new BlogListItem(p.Slug, p.Title, p.Excerpt, p.PublishedUtc))
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<BlogPostDetail?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await _db.BlogPosts
            .Where(p => p.Published && p.Slug == slug)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (post is null)
        {
            return null;
        }

        // Rendered here rather than in the view so Markdig stays out of the views and the
        // detail hands the page ready-to-output HTML.
        return new BlogPostDetail(
            post.Slug,
            post.Title,
            _markdown.ToHtml(post.MarkdownBody),
            post.PublishedUtc,
            post.SeoTitle,
            post.SeoDescription,
            post.Excerpt);
    }
}
