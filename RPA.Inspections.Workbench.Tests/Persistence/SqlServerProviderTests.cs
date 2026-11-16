using System.Data.Entity;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench.Tests.Persistence;

/// <summary>
/// Runs the current EF6 model and the Microsoft.Data.SqlClient provider against a real SQL Server (LocalDB, a
/// scratch database that is dropped afterwards). Skipped when LocalDB is not available. It proves the provider
/// path works end-to-end (real TCP connection, schema creation, CRUD); it cannot prove Azure SQL managed-identity
/// access.
///
/// This deliberately does not replay Workbench's historical, deployment-managed EF6 migrations (the main project
/// excludes `Migrations\**` from compilation - see `ArchivedMigrationsAndEfCoreBaselineAreNotExecutable` - because
/// the schema is released separately by the DBA/pipeline, never by the running application). Resurrecting those
/// migrations here, purely for a test, would contradict that established boundary. Instead the scratch schema is
/// created directly from the current `WorkbenchContext` EF6 model, which still exercises the same
/// Microsoft.Data.SqlClient provider path (`WorkbenchDbConfiguration`, `MicrosoftSqlProviderServices`,
/// `MicrosoftSqlAzureExecutionStrategy`) used against real Azure SQL / LocalDB.
/// </summary>
[TestFixture]
[Category("SqlServer")]
public class SqlServerProviderTests
{
    private const string Server = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=15";

    private string database;
    private string connection;

    private static void Run(string connectionString, string sql)
    {
        using var sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();
        using var command = sqlConnection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    [OneTimeSetUp]
    public void CreateScratchDatabase()
    {
        database = "WorkbenchTest_" + Guid.NewGuid().ToString("N");
        connection = $"{Server};Database={database};MultipleActiveResultSets=True";

        try
        {
            Run($"{Server};Database=master", $"CREATE DATABASE [{database}]");
        }
        catch (SqlException exception)
        {
            Assert.Ignore("LocalDB is not available: " + exception.Message);
        }

        using var context = new WorkbenchContext(connection);
        context.Database.CreateIfNotExists();
    }

    [OneTimeTearDown]
    public void DropScratchDatabase()
    {
        if (database == null) return;

        SqlConnection.ClearAllPools();

        try
        {
            Run($"{Server};Database=master",
                $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END");
        }
        catch (SqlException)
        {
            // Best-effort clean-up of a scratch database.
        }
    }

    [Test]
    public void Data_round_trips_through_the_provider()
    {
        var holdingId = Guid.NewGuid();

        using (var db = new WorkbenchContext(connection))
        {
            db.Holding.Add(new Holding { holdingId = holdingId, cphNumber = "12/345/6789", primaryName = "Provider test holding" });
            db.SaveChanges();
        }

        using (var db = new WorkbenchContext(connection))
        {
            var holding = db.Holding.Single(x => x.holdingId == holdingId);

            Assert.AreEqual("Provider test holding", holding.primaryName);
            Assert.IsTrue(holding.Active);
        }
    }

    [Test]
    public void The_connection_created_by_the_context_is_a_real_Microsoft_Data_SqlClient_connection()
    {
        using var db = new WorkbenchContext(connection);

        Assert.That(db.Database.Connection, Is.TypeOf<SqlConnection>());
        db.Database.Connection.Open();

        Assert.AreEqual(System.Data.ConnectionState.Open, db.Database.Connection.State);
    }

    [Test]
    public void A_missing_database_is_never_created_by_the_runtime()
    {
        var missing = "WorkbenchMissing_" + Guid.NewGuid().ToString("N");

        using (var db = new WorkbenchContext($"{Server};Database={missing}"))
        {
            Assert.Catch(() => db.Holding.ToList());
        }

        SqlConnection.ClearAllPools();

        using var master = new SqlConnection($"{Server};Database=master");
        master.Open();
        using var command = master.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{missing}'";

        Assert.AreEqual(0, (int)command.ExecuteScalar());
    }
}
