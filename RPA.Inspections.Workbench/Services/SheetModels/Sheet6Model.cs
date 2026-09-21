using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet6Model : SheetModelBase
    {
        public CattleDetailsCondensed cattleDetails;

        public Sheet6Model(ExcelRange cells) : base(cells)
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
                    var cattleDetails = new CattleDetailsCondensed(cattle, EarTagBrokenDown);
                    rowStart = Apply(rowStart, cattleDetails);


                }

            }
        }

        public int Apply(int rowStart, CattleDetailsCondensed cattleDetails)
        {
            SetCell("A" + rowStart, cattleDetails.earTag);
            SetCell("B" + rowStart, cattleDetails.herdMark);
            SetCell("C" + rowStart, cattleDetails.checkDigit);
            SetCell("D" + rowStart, cattleDetails.animalNumber);
            SetCell("E" + rowStart, cattleDetails.breed);
            SetCell("F" + rowStart, cattleDetails.gender);
            SetCell("G" + rowStart, cattleDetails.birthDate);

            return rowStart;
        }

    }
}