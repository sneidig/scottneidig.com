using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// The contact inbox. Read-only apart from delete: enquiries come in through the public form
/// and are never edited here, only read and cleared.
/// </summary>
[Area("Admin")]
[Authorize]
public class MessagesController : Controller
{
    private readonly IContactService _contact;

    public MessagesController(IContactService contact) => _contact = contact;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Messages";
        return View(await _contact.GetAllAsync(ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await _contact.DeleteAsync(id, ct)
            ? RedirectToAction(nameof(Index))
            : NotFound();
}
