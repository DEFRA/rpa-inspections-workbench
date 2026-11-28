using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.Tests.Data.Mock;

namespace RPA.Inspections.Workbench.Tests.DAL.Mock
{
    // Real PeopleContext over in-memory SQLite. Production never writes to the People database; the tests seed it directly.
    public class MockPeopleContext : IDisposable
    {
        private readonly SqliteConnection connection = new("DataSource=:memory:;Foreign Keys=False");

        public PeopleContext Context { get; }

        public MockPeopleContext(bool seed = true)
        {
            connection.Open();
            Context = new PeopleContext(new DbContextOptionsBuilder<PeopleContext>().UseSqlite(connection).Options);
            Context.Database.EnsureCreated();
            if (seed)
            {
                Context.Locations.AddRange(LocationData.Data());
                Context.People.AddRange(PeopleData.Data());
                Context.SaveChanges();
                Context.Managers.AddRange(ManagerData.Data());
                Context.SaveChanges();
                Context.ChangeTracker.Clear();
            }
        }

        public void Dispose()
        {
            Context.Dispose();
            connection.Dispose();
        }
    }
}

