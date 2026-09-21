using RPA.Inspections.Workbench.Models;
using System.Data.Entity;

namespace RPA.Inspections.Workbench.DAL
{
    public interface IPeopleContext
    {
        DbSet<Person> People { get; set; }
        DbSet<Location> Locations { get; set; }
        DbSet<Manager> Managers { get; set; }
    }
}