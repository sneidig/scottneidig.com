using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A submitted contact form message. Saved before any email is attempted,
/// so a mail failure never loses the enquiry.
/// </summary>
public class ContactMessage
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(200)]
    public string Email { get; set; } = "";

    [MaxLength(5000)]
    public string Message { get; set; } = "";

    public DateTime CreatedUtc { get; set; }
}
