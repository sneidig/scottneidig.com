namespace ScottNeidig.Web.Services;

public interface IErrorLogService
{
    /// <summary>
    /// Records an unhandled exception. Intended to be called from the error handler, so it must
    /// never throw back at the caller: a logging failure cannot be allowed to mask the original
    /// error. Implementations swallow their own failures (logging them via ILogger instead).
    /// </summary>
    Task LogAsync(Exception exception, string? path, string? method, int statusCode,
        string? requestId, CancellationToken ct = default);

    /// <summary>Most recent entries first.</summary>
    Task<List<ErrorLogSummary>> GetRecentAsync(int take = 200, CancellationToken ct = default);

    /// <summary>Deletes every entry. Returns how many were removed.</summary>
    Task<int> ClearAsync(CancellationToken ct = default);
}
