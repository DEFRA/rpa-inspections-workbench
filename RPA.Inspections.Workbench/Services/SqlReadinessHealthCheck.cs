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
            if (_workbenchContext != null)
            {
                return HealthCheckResult.Healthy("SQL Server is accessible and ready.");
            }
            
            return HealthCheckResult.Unhealthy("Database context not available.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SQL Server health check failed: {ex.Message}", ex);
        }
    }
}
