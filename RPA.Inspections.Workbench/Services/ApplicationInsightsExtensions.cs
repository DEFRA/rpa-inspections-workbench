namespace RPA.Inspections.Workbench.Services;

public static class ApplicationInsightsExtensions
{
    public static IServiceCollection AddWorkbenchApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        // Application Insights is optional - only configure if instrumentation key is provided
        var instrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
        
        if (!string.IsNullOrWhiteSpace(instrumentationKey))
        {
            // Telemetry can be added here when Application Insights package is available
            // services.AddApplicationInsightsTelemetry(instrumentationKey);
        }
        
        return services;
    }
}
