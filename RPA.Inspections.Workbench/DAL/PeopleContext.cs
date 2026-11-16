using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench;

[ExcludeFromCodeCoverage]
[DbConfigurationType(typeof(WorkbenchDbConfiguration))]
public partial class PeopleContext : DbContext, IPeopleContext
{
    static PeopleContext()
    {
        Database.SetInitializer<PeopleContext>(null);
    }

    public PeopleContext(string connectionString) : this(new SqlConnection(connectionString), true)
    {
    }

    public PeopleContext(DbConnection connection, bool contextOwnsConnection)
        : base(connection, contextOwnsConnection)
    {
    }

    public virtual DbSet<Location> Locations { get; set; }
    public virtual DbSet<Manager> Managers { get; set; }
    public virtual DbSet<Person> People { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Properties<DateTime>().Configure(property => property.HasColumnType("datetime"));
        modelBuilder.Properties<decimal>().Configure(property => property.HasPrecision(18, 2));
        modelBuilder.Entity<Person>().ToTable("People", "OLA");
        modelBuilder.Entity<Location>().ToTable("Locations", "OLA");
        modelBuilder.Entity<Manager>().ToTable("Managers", "OLA");
        modelBuilder.Entity<Person>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<Location>().Property(x => x.id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<Manager>().HasKey(x => new { x.personId, x.managerId });
        modelBuilder.Entity<Person>().HasRequired(x => x.Location).WithMany(x => x.People)
            .HasForeignKey(x => x.LocationId).WillCascadeOnDelete(true);
        modelBuilder.Entity<Manager>().HasRequired(x => x.Person).WithMany(x => x.Managers)
            .HasForeignKey(x => x.personId).WillCascadeOnDelete(true);
    }
}
