using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin landing page. Placeholder until the CRUD sections land.
/// </summary>
[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View();
    }
}
