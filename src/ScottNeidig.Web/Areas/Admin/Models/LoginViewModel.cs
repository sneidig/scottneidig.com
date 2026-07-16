using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Areas.Admin.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    /// <summary>Where to send the user after login. Validated as a local URL before use.</summary>
    public string? ReturnUrl { get; set; }
}
