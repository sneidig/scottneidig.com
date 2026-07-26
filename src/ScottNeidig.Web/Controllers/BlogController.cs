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

        return View(await _blog.GetPublishedAsync(ct));
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
