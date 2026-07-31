using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// Read-only view of recorded unhandled exceptions, plus a clear. Exists so production errors
/// can be diagnosed from the admin without server or log-file access.
/// </summary>
[Area("Admin")]
[Authorize]
public class LogsController : Controller
{
    private readonly IErrorLogService _errorLog;

    public LogsController(IErrorLogService errorLog) => _errorLog = errorLog;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Error log";
        return View(await _errorLog.GetRecentAsync(ct: ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _errorLog.ClearAsync(ct);
        return RedirectToAction(nameof(Index));
    }
}
