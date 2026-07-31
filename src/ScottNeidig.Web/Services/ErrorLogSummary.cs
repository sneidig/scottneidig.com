namespace ScottNeidig.Web.Services;

/// <summary>
/// One error-log row for the admin list. Carries the stack trace too, shown collapsed per row,
/// so there is no separate detail round-trip for a low-volume log.
/// </summary>
public record ErrorLogSummary(
    int Id,
    DateTime CreatedUtc,
    string? Path,
    string? Method,
    int StatusCode,
    string? ExceptionType,
    string Message,
    string? StackTrace,
    string? RequestId);
