using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet5Model : SheetModelBase
    {
        public CattleDetails cattleDetails;

        public Sheet5Model(ExcelRange cells) : base(cells)
        {
         
        }

        public void ApplyCattle(int rowStart, List<Cattle> cattleList)
        {
            foreach (var cattle in cattleList)
            {
                //Animal List 1 (And Condensed List)

                if (cattle.listNumber == "LIST1")
                {


                    List<string> EarTagBrokenDown = Functions.EarTag.EarTagBreakdown(cattle.earTag); 
                    var cattleDetails = new CattleDetails(cattle, EarTagBrokenDown);
                    rowStart = Apply(rowStart, cattleDetails);


                }

            }
        }

        public int Apply(int rowStart, CattleDetails cattleDetails)
        {
            SetCell("C" + rowStart, cattleDetails.earTag);
            SetCell("D" + rowStart, cattleDetails.herdMark);
            SetCell("E" + rowStart, cattleDetails.checkDigit);
            SetCell("F" + rowStart, cattleDetails.animalNumber);
            SetCell("G" + rowStart, cattleDetails.breed);
            SetCell("H" + rowStart, cattleDetails.gender);
            SetCell("I" + rowStart, cattleDetails.birthDate);
            SetCell("J" + rowStart, cattleDetails.onDate);
            SetCell("K" + rowStart, cattleDetails.offDate);
            SetCell("L" + rowStart, cattleDetails.deathDate);
            SetCell("M" + rowStart, cattleDetails.damId);
            SetCell("N" + rowStart, cattleDetails.passport);

            return ++rowStart;

        }
    }
}