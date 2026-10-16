using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace RPA.Inspections.Workbench.DAL
{
    // Code-based replacement for the <entityFramework> section of Web.config.
    public class WorkbenchDbConfiguration : DbConfiguration
    {
        public WorkbenchDbConfiguration()
        {
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
            SetDefaultConnectionFactory(new SqlConnectionFactory());
        }
    }
}
