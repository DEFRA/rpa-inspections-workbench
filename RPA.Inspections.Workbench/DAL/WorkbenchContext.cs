using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.DAL
{
    [ExcludeFromCodeCoverage]
    public class WorkbenchContext : DbContext, IWorkbenchContext
    {

        public WorkbenchContext() : base("WorkbenchContext")
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