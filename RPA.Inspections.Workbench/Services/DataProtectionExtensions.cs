using Microsoft.AspNetCore.DataProtection;

namespace RPA.Inspections.Workbench.Services;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddWorkbenchDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var dataProtection = services.AddDataProtection();
        
        // Set application name for data protection
        dataProtection.SetApplicationName("RPA.Inspections.Workbench");

        return services;
    }
}
