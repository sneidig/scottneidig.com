using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// Read-only view of recorded unhandled exceptions, plus a clear. Exists so production errors
/// can be diagnosed from the admin without server or log-file access.
///
/// Named "Errors", not "Logs", deliberately: the standard .NET .gitignore ignores any [Ll]ogs/
/// directory (meant for build logs), which silently kept a Views/Logs/ folder out of the repo
/// and out of the deploy, so its view was "not found" in production.
/// </summary>
[Area("Admin")]
[Authorize]
public class ErrorsController : Controller
{
    private readonly IErrorLogService _errorLog;

    public ErrorsController(IErrorLogService errorLog) => _errorLog = errorLog;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Error log";

        try
        {
            return View(await _errorLog.GetRecentAsync(ct: ct));
        }
        catch (Exception ex)
        {
            // A diagnostics page must never itself return the generic 500. The likeliest cause
            // is the table not existing yet (migration not applied), so show why rather than
            // hiding it behind the error page.
            ViewData["LogError"] = ex.Message;
            return View(new List<ErrorLogSummary>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _errorLog.ClearAsync(ct);
        return RedirectToAction(nameof(Index));
    }
}
