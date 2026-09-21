using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Extensions;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public class FilterService : IFilterService
    {
        IWorkbenchContext db;
        IPeopleContext Pdb;



        public FilterService()
        {
            this.db = new WorkbenchContext();
            this.Pdb = new PeopleContext();
        }

        public FilterService(IWorkbenchContext context, IPeopleContext Pcontext)
        {
            this.db = context;
            this.Pdb = Pcontext;
        }

        public List<Holding> Filterholdings()
        {
            List<Control> ActiveSchemeYears = db.Control.Where(x => x.Active == true && x.Property == "SchemeYr").ToList();

            List<Holding> HoldingList = new List<Holding>();

            foreach (var SchemeYr in ActiveSchemeYears)
            {
                List<Holding> HoldingListYear = db.Holding.Where(YearExtensions.MatchesReference(SchemeYr.Value)).ToList();

                foreach (var holding in HoldingListYear)
                {
                    HoldingList.Add(holding);
                }
            }

            return HoldingList;
        }
    }
}