using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace RPA.Inspections.Workbench.DAL;

public sealed class WorkbenchDbConfiguration : DbConfiguration
{
    public WorkbenchDbConfiguration()
    {
        DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
        SetProviderFactory("System.Data.SqlClient", SqlClientFactory.Instance);
        SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
    }
}
