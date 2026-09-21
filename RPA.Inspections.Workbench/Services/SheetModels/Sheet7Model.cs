using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet7Model : SheetModelBase
    {
        public CattleDetails cattleDetails;

        public Sheet7Model(ExcelRange cells) : base(cells)
        {

        }

        public int ApplyCattle(int rowStart, List<Cattle> cattleList, string selection)
        {
            foreach (var cattle in cattleList)
            {
                //Animal List 1 (And Condensed List)

                if (cattle.listNumber == "LIST2")
                {


                    List<string> EarTagBrokenDown = Functions.EarTag.EarTagBreakdown(cattle.earTag);
                    var cattleDetails = new CattleDetails(cattle, EarTagBrokenDown);
                    rowStart = Apply(rowStart, cattleDetails, selection);


                }

            }

            return rowStart;
        }

        public int Apply(int rowStart, CattleDetails cattleDetails, string selection)
        {
            SetCell("A" + rowStart, cattleDetails.earTag);
            SetCell("B" + rowStart, cattleDetails.herdMark);
            SetCell("C" + rowStart, cattleDetails.checkDigit);
            SetCell("D" + rowStart, cattleDetails.animalNumber);
            SetCell("E" + rowStart, selection);
            SetCell("F" + rowStart, cattleDetails.breed);
            SetCell("G" + rowStart, cattleDetails.gender);
            SetCell("H" + rowStart, cattleDetails.birthDate);
            SetCell("I" + rowStart, cattleDetails.onDate);
            SetCell("J" + rowStart, cattleDetails.offDate);
            SetCell("K" + rowStart, cattleDetails.deathDate);
            SetCell("L" + rowStart, cattleDetails.damId);
            SetCell("M" + rowStart, cattleDetails.passport);

            return ++rowStart;
        }
    }
}