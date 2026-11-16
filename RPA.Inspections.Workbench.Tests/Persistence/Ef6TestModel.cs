using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using RPA.Inspections.Workbench.DAL;

namespace RPA.Inspections.Workbench.Tests.Persistence;

internal static class Ef6TestModel
{
    [ModuleInitializer]
    internal static void RegisterInMemoryProvider() =>
        Effort.Provider.EffortProviderConfiguration.RegisterProvider();

    internal static readonly XNamespace StoreNamespace = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
    private static readonly Lazy<DbModel> Workbench = new(() => Build<WorkbenchContext>());
    private static readonly Lazy<DbModel> People = new(() => Build<PeopleContext>());

    internal static DbModel Model<T>() where T : DbContext =>
        typeof(T) == typeof(WorkbenchContext) ? Workbench.Value : People.Value;

    internal static XDocument Edmx<T>() where T : DbContext
    {
        var document = new XDocument();
        using (var writer = document.CreateWriter())
            EdmxWriter.WriteEdmx(Model<T>(), writer);
        return document;
    }

    internal static XElement Storage<T>() where T : DbContext =>
        Edmx<T>().Descendants(StoreNamespace + "Schema").Single();

    internal static DbContext QueryContext<T>() where T : DbContext
    {
        Database.SetInitializer<DbContext>(null);
        return new DbContext(new SqlConnection("Server=unused;Database=metadata_only;Integrated Security=true"),
            Model<T>().Compile(), contextOwnsConnection: true);
    }

    private static DbModel Build<T>() where T : DbContext
    {
        var builder = new DbModelBuilder();
        foreach (var property in typeof(T).GetProperties()
                     .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
            builder.RegisterEntityType(property.PropertyType.GenericTypeArguments.Single());

        using var context = (T)Activator.CreateInstance(typeof(T),
            "Server=unused;Database=metadata_only;Integrated Security=true");
        // Use the production mapping hook, but supply a manifest token so model building never opens SQL.
        typeof(T).GetMethod("OnModelCreating", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(context, new object[] { builder });
        return builder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
    }

    internal static void DetachAll(this DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().ToArray())
            entry.State = EntityState.Detached;
    }
}
