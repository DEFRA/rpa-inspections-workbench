using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench.DAL;

[ExcludeFromCodeCoverage]
public class WorkbenchContext : DbContext, IWorkbenchContext
{
    public WorkbenchContext(DbContextOptions<WorkbenchContext> options) : base(options)
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

    // EF6 stored datetime (not datetime2) columns; keep that so the existing schema is reused unchanged.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("datetime");
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("IW");

        modelBuilder.Entity<Holding>().HasKey(x => x.holdingId);
        modelBuilder.Entity<Cattle>().HasKey(x => x.cattleId);
        modelBuilder.Entity<Breed>().HasKey(x => x.breedId);
        modelBuilder.Entity<LinkedHolding>().HasKey(x => x.linkedHoldingId);
        modelBuilder.Entity<PackRequested>().HasKey(x => x.packRequestedId);
        modelBuilder.Entity<Inspector>().HasKey(x => x.inspectorId);
        modelBuilder.Entity<Control>().HasKey(x => x.ControlId);
        modelBuilder.Entity<BcmsRequested>().HasKey(x => x.BcmsPackRequestedId);
        modelBuilder.Entity<SchemeNameDrop>().HasKey(x => x.SchemeNameDropId);

        modelBuilder.Entity<Holding>().Property(x => x.holdingId).ValueGeneratedNever();
        modelBuilder.Entity<Cattle>().Property(x => x.cattleId).ValueGeneratedNever();
        modelBuilder.Entity<Breed>().Property(x => x.breedId).ValueGeneratedNever();
        modelBuilder.Entity<LinkedHolding>().Property(x => x.linkedHoldingId).ValueGeneratedNever();
        modelBuilder.Entity<PackRequested>().Property(x => x.packRequestedId).ValueGeneratedNever();
        modelBuilder.Entity<Inspector>().Property(x => x.inspectorId).ValueGeneratedNever();
        modelBuilder.Entity<Control>().Property(x => x.ControlId).ValueGeneratedNever();
        modelBuilder.Entity<BcmsRequested>().Property(x => x.BcmsPackRequestedId).ValueGeneratedNever();
        modelBuilder.Entity<SchemeNameDrop>().Property(x => x.SchemeNameDropId).ValueGeneratedOnAdd();

        // Non-key identity column. SetModified() marks every property modified, so never write it back.
        modelBuilder.Entity<PackRequested>().Property(x => x.Id).ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        modelBuilder.Entity<Cattle>().HasOne(x => x.Breed).WithMany()
            .HasForeignKey(x => x.breedId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Cattle>().HasOne(x => x.Holding).WithMany()
            .HasForeignKey(x => x.holdingId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LinkedHolding>().HasOne(x => x.Holding).WithMany()
            .HasForeignKey(x => x.holdingId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PackRequested>().HasOne(x => x.Holding).WithMany()
            .HasForeignKey(x => x.holdingId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Holding>().HasOne(x => x.SchemeNameDrop).WithMany()
            .HasForeignKey(x => x.SchemeNameDropId).IsRequired(false).OnDelete(DeleteBehavior.ClientSetNull);
    }

    public virtual void SetModified(object obj)
    {
        Entry(obj).State = EntityState.Modified;
    }

    // EF Core does not run DataAnnotations on SaveChanges (EF6 did). Validate added and modified entities so
    // the Holding rules (CPH/SBI/selection method/scheme year/staff number) are still enforced.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateTrackedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateTrackedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected virtual void ValidateTrackedEntities()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            Validator.ValidateObject(entry.Entity, new ValidationContext(entry.Entity), validateAllProperties: true);
        }
    }
}

