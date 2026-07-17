using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ScottNeidig.Web.Data;

namespace ScottNeidig.Web.Health;

/// <summary>
/// Reports the app healthy only if it can actually reach the database. A liveness ping that
/// returns 200 while the DB is down is worse than useless: it tells a monitor everything's fine
/// right up until the first page load fails. Written as a custom check to avoid pulling in the
/// EF health-check package for one line.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public DatabaseHealthCheck(AppDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to the database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check threw.", ex);
        }
    }
}
