using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Controllers;

[Route("blog")]
public class BlogController : Controller
{
    /// <summary>How many related projects a post shows when its category feeds a service.</summary>
    private const int RelatedCount = 3;

    private readonly IBlogService _blog;
    private readonly IProjectService _projects;

    public BlogController(IBlogService blog, IProjectService projects)
    {
        _blog = blog;
        _projects = projects;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Blog";
        ViewData["Description"] =
            "Writing on nopCommerce, .NET, and building for the web, by Scott Neidig.";

        return View(new BlogListViewModel
        {
            Posts = await _blog.GetPublishedAsync(ct: ct),
            Topics = await _blog.GetTopicsAsync(ct)
        });
    }

    [HttpGet("category/{slug}")]
    public async Task<IActionResult> Category(string slug, CancellationToken ct)
    {
        var topics = await _blog.GetTopicsAsync(ct);

        // An unknown topic, or one with no published posts, isn't a real page.
        var selected = topics.FirstOrDefault(t => t.Slug == slug);
        if (selected is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"{selected.Name} posts";
        ViewData["Description"] = $"Writing on {selected.Name} by Scott Neidig.";

        return View(nameof(Index), new BlogListViewModel
        {
            Posts = await _blog.GetPublishedAsync(slug, ct),
            Topics = topics,
            SelectedTopic = selected
        });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Post(string slug, CancellationToken ct)
    {
        var post = await _blog.GetPublishedBySlugAsync(slug, ct);
        if (post is null)
        {
            return NotFound();
        }

        ViewData["Title"] = post.PageTitle;
        ViewData["Description"] = post.PageDescription;

        // When the post's category feeds a service page, pull that page's work to show as proof
        // at the bottom. Only queried when there's a category with a slug to match.
        var relatedProjects = post.CategorySlug is null
            ? []
            : await _projects.GetPublishedAsync(post.CategorySlug, RelatedCount, ct);

        return View(new BlogPostViewModel
        {
            Post = post,
            RelatedProjects = relatedProjects
        });
    }
}
