using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Models;
using ScottNeidig.Web.Services;

namespace ScottNeidig.Web.Controllers;

/// <summary>
/// The bottom of the funnel. Every service page's "Schedule a consultation" lands here.
/// </summary>
[Route("contact")]
public class ContactController : Controller
{
    private readonly IContactService _contact;
    private readonly ILogger<ContactController> _log;

    public ContactController(IContactService contact, ILogger<ContactController> log)
    {
        _contact = contact;
        _log = log;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Contact";
        ViewData["Description"] =
            "Get in touch with Scott Neidig about nopCommerce work or a small-business website.";

        return View(new ContactFormModel());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Contact";

        // Honeypot: a human never sees the Website field, so a filled one is a bot. Return the
        // same success view a real submit gets, so the bot can't tell it was caught and tune.
        if (!string.IsNullOrEmpty(model.Website))
        {
            _log.LogInformation("Contact form honeypot tripped; enquiry discarded");
            return View("Sent");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _contact.CreateAsync(model.Name, model.Email, model.Message, ct);

        // Redirect after POST, so a refresh on the thank-you page doesn't resubmit. The
        // success message is its own GET rather than a re-rendered view for the same reason.
        return RedirectToAction(nameof(Sent));
    }

    [HttpGet("sent")]
    public IActionResult Sent()
    {
        ViewData["Title"] = "Message sent";
        return View();
    }
}
