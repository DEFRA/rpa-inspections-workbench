using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet3Model:SheetModelBase
    {
        public CphAddress cphAddress;

        public Sheet3Model(ExcelRange cells) : base(cells)
        {
        }

        public void ApplyLinkedHoldings(int rowStart, List<LinkedHolding> linkedHoldingList)
        {
            foreach (var linkedHolding in linkedHoldingList)
            {
                if (linkedHolding.isLink == true)
                {
                    var cphAddress = new CphAddress(linkedHolding);
                    rowStart = Apply(rowStart, cphAddress);
                }

            }

        }
        

        public int Apply(int rowStart, CphAddress cphAddress)
        {
            SetCell("A"+rowStart, cphAddress.cphNumberLH);
            SetCell("B"+rowStart, cphAddress.primaryNameLH);
            rowStart++;
            SetCell("B"+rowStart, cphAddress.addressLine1LH);
            rowStart++;
            SetCell("B"+rowStart, cphAddress.addressLine2LH);
            rowStart++;
            SetCell("B"+rowStart, cphAddress.addressLine3LH);
            rowStart++;
            SetCell("B"+rowStart, cphAddress.postCodeLH);
            rowStart++;
            SetCell("B"+rowStart, cphAddress.regionLH);

            return rowStart;

        }
    }
}