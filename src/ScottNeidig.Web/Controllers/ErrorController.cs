using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// Real error pages with the right status codes. A missing page has to return 404, not a 200
/// with an error message: browsers, and crawlers especially, act on the status code, and a
/// soft 404 (200 body saying "not found") gets the page indexed as real content.
/// </summary>
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _log;

    public ErrorController(ILogger<ErrorController> log) => _log = log;

    /// <summary>
    /// The exception handler re-executes to here after an unhandled exception. It has already
    /// set a 500 on the response; this logs the real error with context and renders a page.
    /// </summary>
    [Route("/error")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Exception()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        _log.LogError(feature?.Error, "Unhandled exception on {Path}", feature?.Path);

        ViewData["Title"] = "Something went wrong";
        return View("Error", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /// <summary>
    /// Status-code pages re-execute to here (e.g. /error/404). The re-execute middleware keeps
    /// the original status on the response, so returning a view preserves the 404.
    /// </summary>
    [Route("/error/{code:int}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Status(int code)
    {
        if (code == 404)
        {
            ViewData["Title"] = "Page not found";
            return View("NotFound");
        }

        ViewData["Title"] = "Something went wrong";
        return View("Error", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
