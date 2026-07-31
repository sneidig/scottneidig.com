using System.ComponentModel.DataAnnotations;

namespace ScottNeidig.Web.Data.Entities;

/// <summary>
/// A recorded unhandled exception. Written by the error handler so production errors can be
/// read in the admin without server/log access. Message and StackTrace are unbounded
/// (nvarchar(max)); the rest are capped since they are short by nature.
/// </summary>
public class ErrorLogEntry
{
    public int Id { get; set; }

    public DateTime CreatedUtc { get; set; }

    /// <summary>The request path that failed, e.g. /services/nopcommerce.</summary>
    [MaxLength(2048)]
    public string? Path { get; set; }

    [MaxLength(10)]
    public string? Method { get; set; }

    public int StatusCode { get; set; }

    /// <summary>Matches the "Reference" shown on the error page, for cross-referencing.</summary>
    [MaxLength(128)]
    public string? RequestId { get; set; }

    [MaxLength(256)]
    public string? ExceptionType { get; set; }

    public string Message { get; set; } = "";

    public string? StackTrace { get; set; }
}
