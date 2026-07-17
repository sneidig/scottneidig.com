using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Models;

/// <summary>
/// The contact form. Kept to three fields on purpose: every field is friction, and a short
/// form converts better than a thorough one. Phone and "how did you find me" can be added
/// later if the leads justify the extra friction.
/// </summary>
public class ContactFormModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Tell me a little about what you need.")]
    [MaxLength(5000)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "What can I help with?")]
    public string Message { get; set; } = "";

    /// <summary>
    /// Honeypot. A real person never sees this field (hidden in CSS), so anything filling it
    /// in is a bot. Named to look tempting to a script scanning for a contact field. This is
    /// the no-JS, no-third-party spam guard: a CAPTCHA would drag in an external script the
    /// PageSpeed rules forbid, and solving one isn't something we'd do anyway.
    /// </summary>
    [MaxLength(200)]
    public string? Website { get; set; }
}
