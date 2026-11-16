using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench.DAL;

[ExcludeFromCodeCoverage]
[DbConfigurationType(typeof(WorkbenchDbConfiguration))]
public class WorkbenchContext : DbContext, IWorkbenchContext
{
    static WorkbenchContext()
    {
        Database.SetInitializer<WorkbenchContext>(null);
    }

    public WorkbenchContext(string connectionString) : this(new SqlConnection(connectionString), true)
    {
    }

    public WorkbenchContext(DbConnection connection, bool contextOwnsConnection)
        : base(connection, contextOwnsConnection)
    {
    }

    public virtual DbSet<Holding> Holding { get; set; }
    public virtual DbSet<Cattle> Cattle { get; set; }
    public virtual DbSet<Breed> Breed { get; set; }
    public virtual DbSet<LinkedHolding> LinkedHolding { get; set; }
    public virtual DbSet<PackRequested> PackRequested { get; set; }
    public virtual DbSet<Inspector> Inspector { get; set; }
    public virtual DbSet<Control> Control { get; set; }
    public virtual DbSet<BcmsRequested> BcmsRequested { get; set; }
    public virtual DbSet<SchemeNameDrop> SchemeNameDrop { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("IW");
        modelBuilder.Properties<DateTime>().Configure(property => property.HasColumnType("datetime"));
        modelBuilder.Properties<decimal>().Configure(property => property.HasPrecision(18, 2));
        modelBuilder.Entity<Holding>().Property(x => x.holdingId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<Cattle>().Property(x => x.cattleId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<Breed>().Property(x => x.breedId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<LinkedHolding>().Property(x => x.linkedHoldingId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<PackRequested>().Property(x => x.packRequestedId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<Inspector>().Property(x => x.inspectorId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<Control>().Property(x => x.ControlId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<BcmsRequested>().Property(x => x.BcmsPackRequestedId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        modelBuilder.Entity<SchemeNameDrop>().Property(x => x.SchemeNameDropId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        modelBuilder.Entity<PackRequested>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        modelBuilder.Entity<Cattle>().HasRequired(x => x.Breed).WithMany()
            .HasForeignKey(x => x.breedId).WillCascadeOnDelete(true);
        modelBuilder.Entity<Cattle>().HasRequired(x => x.Holding).WithMany()
            .HasForeignKey(x => x.holdingId).WillCascadeOnDelete(true);
        modelBuilder.Entity<LinkedHolding>().HasRequired(x => x.Holding).WithMany()
            .HasForeignKey(x => x.holdingId).WillCascadeOnDelete(true);
        modelBuilder.Entity<PackRequested>().HasRequired(x => x.Holding).WithMany()
            .HasForeignKey(x => x.holdingId).WillCascadeOnDelete(true);
        modelBuilder.Entity<Holding>().HasOptional(x => x.SchemeNameDrop).WithMany()
            .HasForeignKey(x => x.SchemeNameDropId).WillCascadeOnDelete(false);
    }

    public virtual void SetModified(object obj)
    {
        Entry(obj).State = EntityState.Modified;
    }
}
