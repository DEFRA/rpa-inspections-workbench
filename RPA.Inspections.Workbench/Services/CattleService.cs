using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public class CattleService:ICattleService
    {

        IWorkbenchContext db;


        public CattleService(IWorkbenchContext context)
        {
            this.db = context;
        }

        //Get a list of all Cattle belonging to this Holding with HoldingId

        public List<Cattle> GetCattleByHoldingId(Guid? HoldingId)
        {
            List<Cattle> model = new List<Cattle>();
            List<Cattle> model2 = new List<Cattle>();

            DateTime offDateTest = DateTime.Now.AddYears(-1);
            DateTime offDeathTest = DateTime.Now.AddDays(-28);
            DateTime offDeathTestL1 = DateTime.Now.AddMonths(-3);
            DateTime DeathParse = new DateTime(1950, 1, 1);

            model = db.Cattle.Where(x => x.holdingId == HoldingId && ((x.listNumber == "LIST1" && (x.deathDate == null || x.deathDate == DeathParse || x.deathDate > offDeathTestL1)) || (x.listNumber == "LIST2" && (x.offDate >= offDateTest && x.deathDate == null || x.offDate >= offDateTest && x.deathDate >= offDeathTest || x.offDate == null && x.deathDate == null || x.offDate == null && x.deathDate >= offDeathTest)))).OrderBy(x => x.listNumber).ThenBy(x => x.earTag).ToList();


            return model;
        }


        //Get all late notifications

        public List<Cattle> GetLateCattle(Guid? HoldingId)
        {
            List<Cattle> model = new List<Cattle>();

            DateTime Current = DateTime.Now;

            model = db.Cattle.Where(x => x.holdingId == HoldingId && x.lateNotification == true && x.notificationDate.Value.Year == Current.Year).OrderBy(x => x.lateType).ThenByDescending(x => x.numberOfDaysLate).ToList();

            return model;

        }

        //Get Animal List using CPH

        public List<Cattle> GetCattleByCPH(string CPHNumber)
        {


            List<Cattle> model = new List<Cattle>();

            Guid HoldingId = db.Holding.Where(x => x.cphNumber == CPHNumber).FirstOrDefault().holdingId;

            model = db.Cattle.Where(x => x.holdingId == HoldingId).OrderBy(x => x.listNumber).ThenBy(x => x.earTag).ToList();

            return model;

        }

        
    }
}