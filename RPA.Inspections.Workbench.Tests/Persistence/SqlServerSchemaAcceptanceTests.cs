using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
[Category("SqlServerAcceptance")]
public class SqlServerSchemaAcceptanceTests
{
    [Test]
    public void WorkbenchExistingSchemaAndNavigationQueriesAreCompatible()
    {
        var connection = RequiredConnection("WORKBENCH_SQL_TEST_CONNECTION");
        using var context = new WorkbenchContext(new DbContextOptionsBuilder<WorkbenchContext>().UseSqlServer(connection).Options);
        VerifyColumns(context);
        context.Holding.AsNoTracking().Include(x => x.SchemeNameDrop).Take(1).ToList();
        context.Cattle.AsNoTracking().Include(x => x.Breed).Include(x => x.Holding).Take(1).ToList();
        context.LinkedHolding.AsNoTracking().Include(x => x.Holding).Take(1).ToList();
        context.PackRequested.AsNoTracking().Include(x => x.Holding).Take(1).ToList();
        context.BcmsRequested.AsNoTracking().Take(1).ToList();
        context.Inspector.AsNoTracking().Take(1).ToList();
        context.Control.AsNoTracking().Take(1).ToList();
    }

    [Test]
    public void PeopleExistingSchemaAndNavigationQueriesAreCompatible()
    {
        var connection = RequiredConnection("PEOPLE_SQL_TEST_CONNECTION");
        using var context = new PeopleContext(new DbContextOptionsBuilder<PeopleContext>().UseSqlServer(connection).Options);
        VerifyColumns(context);
        context.People.AsNoTracking().Include(x => x.Location).Include(x => x.Managers).AsSplitQuery().Take(1).ToList();
        context.Locations.AsNoTracking().Take(1).ToList();
        context.Managers.AsNoTracking().Include(x => x.Person).Take(1).ToList();
    }

    private static string RequiredConnection(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(value))
            Assert.Ignore($"Set {variable} explicitly for read-only SQL Server schema acceptance. No live database parity is claimed without it.");
        return value;
    }

    // Every mapped table and column must already exist: the schema is deployment-managed and never created by the app.
    private static void VerifyColumns(DbContext context)
    {
        var connection = context.Database.GetDbConnection();
        connection.Open();
        foreach (var entity in context.GetService<IDesignTimeModel>().Model.GetEntityTypes())
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table";
            command.Parameters.Add(new SqlParameter("@schema", entity.GetSchema()));
            command.Parameters.Add(new SqlParameter("@table", entity.GetTableName()));
            var actual = new List<string>();
            using (var reader = command.ExecuteReader())
                while (reader.Read()) actual.Add(reader.GetString(0));
            foreach (var property in entity.GetProperties())
                Assert.That(actual, Does.Contain(property.GetColumnName()), $"{entity.GetSchema()}.{entity.GetTableName()}.{property.GetColumnName()}");
        }
    }
}

