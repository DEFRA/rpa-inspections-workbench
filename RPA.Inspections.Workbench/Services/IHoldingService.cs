using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;

namespace RPA.Inspections.Workbench.SL
{
    public interface IHoldingService
    {
        PagedList<Holding> GetHolding(string searchstring, int page, int pageSize);

        PagedList<Holding> GetAllHolding(string searchstring, int page, int pageSize);

        Holding GetHoldingById(Guid? HoldingId);

        Holding GetHoldingByCPH(string CPH);

        List<Person> PersonList(string searchstring);

        List<Holding> GetAllArchivedHoldings(string searchString, List<string> archivedYears, string searchYear);

        Holding GetArchivedHoldingById(Guid? HoldingId);
    }
}
