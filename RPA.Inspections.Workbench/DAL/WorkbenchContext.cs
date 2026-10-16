using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RPA.Inspections.Workbench.DAL
{
    [ExcludeFromCodeCoverage]
    [DbConfigurationType(typeof(WorkbenchDbConfiguration))]
    public class WorkbenchContext : DbContext, IWorkbenchContext
    {
        // Required by DbMigrator, which creates the context itself; the migration configuration supplies the real connection.
        public WorkbenchContext() : base("WorkbenchContext")
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        public WorkbenchContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
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

        public virtual void SetModified(object obj)
        {
            this.Entry(obj).State = EntityState.Modified;
        }

    }
}