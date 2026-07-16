using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScottNeidig.Web.Areas.Admin.Models;

namespace ScottNeidig.Web.Areas.Admin.Controllers;

/// <summary>
/// The way into the admin area. AllowAnonymous is applied per-action rather than to the
/// whole controller: a class-level AllowAnonymous would override the [Authorize] on
/// Logout and silently leave it open.
/// </summary>
[Area("Admin")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Sign in";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["Title"] = "Sign in";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Admin signed in: {Email}", model.Email);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Admin account locked out: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
            return View(model);
        }

        // Deliberately vague: saying which of the two was wrong tells an attacker
        // whether the account exists.
        _logger.LogWarning("Failed admin sign-in attempt for {Email}", model.Email);
        ModelState.AddModelError(string.Empty, "Incorrect email or password.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Only ever redirect to our own URLs. Taking returnUrl straight from the query string
    /// would be an open redirect.
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Dashboard", new { area = "Admin" });
}
