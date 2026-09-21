using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public class LinkedHoldingService : ILinkedHoldingService
    {


        IWorkbenchContext db;

        public LinkedHoldingService(IWorkbenchContext context)
        {
            this.db = context;
        }

        //Get a list of linked Holdings By HoldingID

        public List<LinkedHolding> GetLinkedholdingById(Guid? HoldingId)
        {
            List<LinkedHolding> linkedHoldingList = new List<LinkedHolding>();
                
            linkedHoldingList = db.LinkedHolding.Where(x => x.holdingId == HoldingId).ToList();

            return linkedHoldingList;
        }

        //Get a list of linked Holdings By CPH

        public List<LinkedHolding> GetLinkedholdingByCPH(string CPHNumber)
        {
            Guid? HoldingId = null;

            List<LinkedHolding> linkedHoldingList = new List<LinkedHolding>();

            HoldingId = db.Holding.Where(x => x.cphNumber == CPHNumber).FirstOrDefault().holdingId;

            linkedHoldingList = db.LinkedHolding.Where(x => x.holdingId == HoldingId).ToList();

            return linkedHoldingList;

        }

    }
}