using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ErrorLogService> _log;

    public ErrorLogService(AppDbContext db, ILogger<ErrorLogService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task LogAsync(Exception exception, string? path, string? method, int statusCode,
        string? requestId, CancellationToken ct = default)
    {
        try
        {
            _db.ErrorLogs.Add(new ErrorLogEntry
            {
                CreatedUtc = DateTime.UtcNow,
                Path = path,
                Method = method,
                StatusCode = statusCode,
                RequestId = requestId,
                ExceptionType = exception.GetType().FullName,
                Message = exception.Message,
                // ToString gives the full chain including inner exceptions, which is usually
                // where the real cause is.
                StackTrace = exception.ToString()
            });

            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // The caller is the error handler. If persisting the log fails (e.g. the original
            // error was the database being unreachable), swallow it so the user still gets the
            // error page, and leave a breadcrumb in the standard logger.
            _log.LogError(ex, "Failed to persist error log entry for {Path}", path);
        }
    }

    public Task<List<ErrorLogSummary>> GetRecentAsync(int take = 200, CancellationToken ct = default) =>
        _db.ErrorLogs
            .OrderByDescending(e => e.CreatedUtc)
            .ThenByDescending(e => e.Id)
            .Take(take)
            .Select(e => new ErrorLogSummary(
                e.Id, e.CreatedUtc, e.Path, e.Method, e.StatusCode,
                e.ExceptionType, e.Message, e.StackTrace, e.RequestId))
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<int> ClearAsync(CancellationToken ct = default) =>
        _db.ErrorLogs.ExecuteDeleteAsync(ct);
}
