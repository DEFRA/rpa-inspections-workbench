using System.Data.Common;
using System.Data.Entity;
using System.Xml.Linq;
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
        using var context = new WorkbenchContext(connection);
        VerifySchema(context, Ef6TestModel.Storage<WorkbenchContext>());
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
        using var context = new PeopleContext(connection);
        VerifySchema(context, Ef6TestModel.Storage<PeopleContext>());
        context.People.AsNoTracking().Include(x => x.Location).Include(x => x.Managers).Take(1).ToList();
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

    private static void VerifySchema(DbContext context, XElement storage)
    {
        var ns = Ef6TestModel.StoreNamespace;
        context.Database.Connection.Open();
        foreach (var entity in storage.Elements(ns + "EntityType"))
        {
            var entityName = (string)entity.Attribute("Name");
            var table = storage.Descendants(ns + "EntitySet").Single(x =>
                ((string)x.Attribute("EntityType")).Split('.').Last() == entityName);
            var schemaName = (string)table.Attribute("Schema");
            var tableName = (string)table.Attribute("Table") ?? (string)table.Attribute("Name");
            using var command = context.Database.Connection.CreateCommand();
            command.CommandText = """
                SELECT c.name, t.name, c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity
                FROM sys.columns c
                JOIN sys.tables tb ON tb.object_id = c.object_id
                JOIN sys.schemas s ON s.schema_id = tb.schema_id
                JOIN sys.types t ON t.user_type_id = c.user_type_id
                WHERE s.name = @schema AND tb.name = @table;
                """;
            AddParameter(command, "@schema", schemaName);
            AddParameter(command, "@table", tableName);
            var columns = new Dictionary<string, object[]>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    columns.Add(reader.GetString(0), values);
                }
            }

            foreach (var property in entity.Elements(ns + "Property"))
            {
                var name = (string)property.Attribute("Name");
                var label = $"{schemaName}.{tableName}.{name}";
                Assert.That(columns.ContainsKey(name), Is.True, label);
                var actual = columns[name];
                var storeType = (string)property.Attribute("Type");
                Assert.That(actual[1], Is.EqualTo(storeType), label);
                Assert.That(actual[5], Is.EqualTo((string)property.Attribute("Nullable") != "false"), label);
                Assert.That(actual[6], Is.EqualTo((string)property.Attribute("StoreGeneratedPattern") == "Identity"), label);
                if (storeType is "nvarchar" or "nchar" or "varchar" or "char" or "varbinary" or "binary")
                {
                    var maxLength = (string)property.Attribute("MaxLength");
                    var length = maxLength == null || maxLength == "Max" ? -1 : int.Parse(maxLength) *
                        (storeType is "nvarchar" or "nchar" ? 2 : 1);
                    Assert.That(Convert.ToInt32(actual[2]), Is.EqualTo(length), label);
                }
                if (storeType is "decimal" or "numeric")
                {
                    Assert.That(Convert.ToInt32(actual[3]), Is.EqualTo((int)property.Attribute("Precision")), label);
                    Assert.That(Convert.ToInt32(actual[4]), Is.EqualTo((int)property.Attribute("Scale")), label);
                }
            }

            command.CommandText = """
                SELECT c.name FROM sys.indexes i
                JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                JOIN sys.tables tb ON tb.object_id = i.object_id
                JOIN sys.schemas s ON s.schema_id = tb.schema_id
                WHERE s.name = @schema AND tb.name = @table AND i.is_primary_key = 1
                ORDER BY ic.key_ordinal;
                """;
            var key = new List<string>();
            using (var reader = command.ExecuteReader())
                while (reader.Read()) key.Add(reader.GetString(0));
            Assert.That(key, Is.EqualTo(entity.Element(ns + "Key").Elements(ns + "PropertyRef").Select(x => (string)x.Attribute("Name"))),
                $"{schemaName}.{tableName} primary key");

            foreach (var foreignKey in storage.Elements(ns + "Association").Where(x =>
                         (string)x.Element(ns + "ReferentialConstraint").Element(ns + "Dependent").Attribute("Role") == entityName))
            {
                var constraint = foreignKey.Element(ns + "ReferentialConstraint");
                var principalRole = (string)constraint.Element(ns + "Principal").Attribute("Role");
                var principal = foreignKey.Elements(ns + "End").Single(x => (string)x.Attribute("Role") == principalRole);
                var principalType = ((string)principal.Attribute("Type")).Split('.').Last();
                var principalTable = storage.Descendants(ns + "EntitySet").Single(x =>
                    ((string)x.Attribute("EntityType")).Split('.').Last() == principalType);
                command.CommandText = """
                    SELECT ps.name, pt.name, pc.name, fk.delete_referential_action_desc
                    FROM sys.foreign_keys fk
                    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                    JOIN sys.tables dt ON dt.object_id = fk.parent_object_id
                    JOIN sys.schemas ds ON ds.schema_id = dt.schema_id
                    JOIN sys.columns dc ON dc.object_id = dt.object_id AND dc.column_id = fkc.parent_column_id
                    JOIN sys.tables pt ON pt.object_id = fk.referenced_object_id
                    JOIN sys.schemas ps ON ps.schema_id = pt.schema_id
                    JOIN sys.columns pc ON pc.object_id = pt.object_id AND pc.column_id = fkc.referenced_column_id
                    WHERE ds.name = @schema AND dt.name = @table AND dc.name = @column;
                    """;
                AddParameter(command, "@column", (string)constraint.Element(ns + "Dependent").Element(ns + "PropertyRef").Attribute("Name"));
                using (var reader = command.ExecuteReader())
                {
                    Assert.That(reader.Read(), Is.True, $"{entityName} foreign key");
                    Assert.That(reader.GetString(0), Is.EqualTo((string)principalTable.Attribute("Schema")));
                    Assert.That(reader.GetString(1), Is.EqualTo((string)principalTable.Attribute("Table") ?? (string)principalTable.Attribute("Name")));
                    Assert.That(reader.GetString(2), Is.EqualTo((string)constraint.Element(ns + "Principal").Element(ns + "PropertyRef").Attribute("Name")));
                    Assert.That(reader.GetString(3), Is.EqualTo(
                        (string)principal.Element(ns + "OnDelete")?.Attribute("Action") == "Cascade" ? "CASCADE" : "NO_ACTION"));
                }
                command.Parameters.RemoveAt("@column");
            }
        }
    }

    private static void AddParameter(DbCommand command, string name, string value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
