namespace ScottNeidig.Web.Services;

/// <summary>
/// A contact enquiry for the admin inbox.
/// DTO rather than entity, so it's safe to hand straight to a view.
/// </summary>
public record ContactMessageSummary(
    int Id,
    string Name,
    string Email,
    string Message,
    DateTime CreatedUtc);
