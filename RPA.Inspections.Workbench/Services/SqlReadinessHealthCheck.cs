using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RPA.Inspections.Workbench.DAL;

namespace RPA.Inspections.Workbench.Services;

public class SqlReadinessHealthCheck : IHealthCheck
{
    private readonly IWorkbenchContext _workbenchContext;

    public SqlReadinessHealthCheck(IWorkbenchContext workbenchContext)
    {
        _workbenchContext = workbenchContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Bounded, lightweight query against a small reference table - a real round trip to
            // SQL Server, not just a DI-container null check (which is always true once the
            // context object is constructed, even when the database is unreachable).
            await _workbenchContext.Breed.AnyAsync(cancellationToken);

            return HealthCheckResult.Healthy("SQL Server is accessible and ready.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SQL Server health check failed: {ex.Message}", ex);
        }
    }
}
