using RPA.Inspections.Workbench.Models;
using System.Data.Entity;

namespace RPA.Inspections.Workbench.DAL
{
    public interface IWorkbenchContext
    {
        DbSet<BcmsRequested> BcmsRequested { get; set; }
        DbSet<Breed> Breed { get; set; }
        DbSet<Cattle> Cattle { get; set; }
        DbSet<Control> Control { get; set; }
        DbSet<Holding> Holding { get; set; }
        DbSet<Inspector> Inspector { get; set; }
        DbSet<LinkedHolding> LinkedHolding { get; set; }
        DbSet<PackRequested> PackRequested { get; set; }
        DbSet<SchemeNameDrop> SchemeNameDrop { get; set; }

        void SetModified(object obj);

        int SaveChanges();

        void Dispose();
    }
}