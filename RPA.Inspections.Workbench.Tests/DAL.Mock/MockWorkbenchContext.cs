using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Tests.Data.Mock;

namespace RPA.Inspections.Workbench.Tests.DAL.Mock
{
    // Real WorkbenchContext over an in-memory SQLite database seeded with the shared test data.
    // The connection stays open for the lifetime of the instance because the database vanishes when it closes.
    public class MockWorkbenchContext : IDisposable
    {
        private readonly SqliteConnection connection;

        public WorkbenchContext Context { get; }

        public MockWorkbenchContext(bool seed = true, bool enforceForeignKeys = false)
        {
            // Seed data predates the schema constraints, so keys are only enforced when a test asks for it.
            connection = new SqliteConnection($"DataSource=:memory:;Foreign Keys={enforceForeignKeys}");
            connection.Open();
            Context = NewContext(validate: true);
            Context.Database.EnsureCreated();
            if (seed)
            {
                Seed();
            }
        }

        public WorkbenchContext NewContext() => NewContext(validate: true);

        private WorkbenchContext NewContext(bool validate) =>
            new SqliteWorkbenchContext(new DbContextOptionsBuilder<WorkbenchContext>().UseSqlite(connection).Options, validate);

        // SQLite cannot generate the non-key identity column PackRequested.Id, so the tests assign it instead.
        // The shared test data also predates the Holding validation rules, so seeding skips them.
        private sealed class SqliteWorkbenchContext(DbContextOptions<WorkbenchContext> options, bool validate) : WorkbenchContext(options)
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<RPA.Inspections.Workbench.Models.PackRequested>().Property(x => x.Id).ValueGeneratedNever();
            }

            protected override void ValidateTrackedEntities()
            {
                if (validate)
                {
                    base.ValidateTrackedEntities();
                }
            }
        }

        private void Seed()
        {
            using var seeding = NewContext(validate: false);
            Seed(seeding);
        }
        private static void Seed(WorkbenchContext Context)
        {
            Context.SchemeNameDrop.AddRange(SchemeNameDropData.Data());
            Context.Breed.AddRange(BreedData.Data());
            Context.Control.AddRange(ControlData.Data());
            Context.Inspector.AddRange(InspectorData.Data());
            var holdings = HoldingData.Data();
            holdings.ForEach(h => h.selectionMethod ??= "RANDOM");
            Context.Holding.AddRange(holdings);
            Context.SaveChanges();
            var breedId = BreedData.Data()[0].breedId;
            var cattle = CattleData.Data();
            cattle.ForEach(c => c.breedId = breedId);
            Context.Cattle.AddRange(cattle);
            Context.LinkedHolding.AddRange(LinkedHoldingData.Data());
            Context.PackRequested.AddRange(PackRequestedData.Data());
            Context.BcmsRequested.AddRange(BcmsRequestedData.Data());
            Context.SaveChanges();
            Context.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            Context.Dispose();
            connection.Dispose();
        }
    }
}







