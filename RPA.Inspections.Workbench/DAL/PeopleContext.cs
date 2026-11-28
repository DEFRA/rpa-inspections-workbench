using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench;

// Externally owned People database: mapped read-only (IPeopleContext exposes no SaveChanges) and never migrated.
[ExcludeFromCodeCoverage]
public partial class PeopleContext : DbContext, IPeopleContext
{
    public PeopleContext(DbContextOptions<PeopleContext> options) : base(options)
    {
    }

    public virtual DbSet<Location> Locations { get; set; }
    public virtual DbSet<Manager> Managers { get; set; }
    public virtual DbSet<Person> People { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("datetime");
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().ToTable("People", "OLA");
        modelBuilder.Entity<Location>().ToTable("Locations", "OLA");
        modelBuilder.Entity<Manager>().ToTable("Managers", "OLA");
        modelBuilder.Entity<Person>().HasKey(x => x.Id);
        modelBuilder.Entity<Location>().HasKey(x => x.id);
        modelBuilder.Entity<Person>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Location>().Property(x => x.id).ValueGeneratedNever();
        modelBuilder.Entity<Manager>().HasKey(x => new { x.personId, x.managerId });
        modelBuilder.Entity<Person>().HasOne(x => x.Location).WithMany(x => x.People)
            .HasForeignKey(x => x.LocationId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Manager>().HasOne(x => x.Person).WithMany(x => x.Managers)
            .HasForeignKey(x => x.personId).IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}
