using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Controllers;

[Route("blog")]
public class BlogController : Controller
{
    private readonly IBlogService _blog;

    public BlogController(IBlogService blog) => _blog = blog;

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

        return View(post);
    }
}
