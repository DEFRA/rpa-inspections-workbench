using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class CphAddress
    {
        public string cphNumberLH { get; set; }
        public string primaryNameLH { get; set; }
        public string addressLine1LH { get; set; }
        public string addressLine2LH { get; set; }
        public string addressLine3LH { get; set; }
        public string regionLH { get; set; }
        public string postCodeLH { get; set; }

        public CphAddress(LinkedHolding linkedHolding)
        {
            cphNumberLH = linkedHolding.CPHNumberLH;
            primaryNameLH = linkedHolding.primaryNameLH;
            addressLine1LH = linkedHolding.addressLine1LH;
            addressLine2LH = linkedHolding.addressLine2LH;
            addressLine3LH = linkedHolding.addressLine3LH;
            postCodeLH = linkedHolding.PostCodeLH;
            regionLH = linkedHolding.regionLH;
        }
    }
}