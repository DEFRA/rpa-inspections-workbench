using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet9Model : SheetModelBase
    {
        public CattleDetailsHistoric CattleDetailsHistoric;

        public LateCattleCount LateCattleCount { get; set; }

        public LateCattleCalculator LateCattleCalculator { get; set; }

        public Sheet9Model(ExcelRange cells) : base(cells)
        {

        }
        public void ApplyCattle(int rowStart, List<Cattle> cattleList, DateTime timeNow, IHolidayService holidayService)
        {
            foreach (var cattle in cattleList)
            {
                //Late Cattle List
                DateTime notificationDate = cattle.notificationDate.HasValue ? cattle.notificationDate.Value : DateTime.Now;
                DateTime lateDate = cattle.lateDate.HasValue ? cattle.lateDate.Value : DateTime.Now;

                var cattleDetailsHistoric = new CattleDetailsHistoric(cattle, notificationDate, lateDate, holidayService);

                rowStart = Apply(rowStart, cattleDetailsHistoric);
            }
        }

        public void CalculateCattleStats(List<Cattle> cattleList, int totalMovesThisYear, IHolidayService holidayService)
        {
            var lateCattleCount = new LateCattleCount();
            var lateCattleCalculator = new LateCattleCalculator(holidayService);

            lateCattleCount.TotalMovesThisYear = totalMovesThisYear;

            foreach (var cattle in cattleList)
            {
                lateCattleCount = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService);
            }

            if (lateCattleCount.FMXC1Count + lateCattleCount.DDXC1Count + lateCattleCount.LRXC1Count + lateCattleCount.IMPXC1Count > 0 && lateCattleCount.FMXC2Count + lateCattleCount.DDXC2Count + lateCattleCount.LRXC2Count + lateCattleCount.IMPXC2Count == 0)
            {
                lateCattleCount.HEA = "Yes";
            }
            else if (lateCattleCount.FMXC2Count + lateCattleCount.DDXC2Count + lateCattleCount.LRXC2Count + lateCattleCount.IMPXC2Count > 0)
            {
                lateCattleCount.HEA = "No";
            }
            else
            {
                lateCattleCount.HEA = "N/A";
            }

            ApplyStats(lateCattleCount);
        }

        public int Apply(int rowStart, CattleDetailsHistoric cattleDetailsHistoric)
        {
            SetCell("B" + rowStart, cattleDetailsHistoric.earTag);
            SetCell("C" + rowStart, cattleDetailsHistoric.breed);
            SetCell("D" + rowStart, cattleDetailsHistoric.gender);
            SetCell("E" + rowStart, cattleDetailsHistoric.birthDate);
            SetCell("F" + rowStart, cattleDetailsHistoric.onDate);
            SetCell("G" + rowStart, cattleDetailsHistoric.offDate);
            SetCell("H" + rowStart, cattleDetailsHistoric.deathDate);
            SetCell("I" + rowStart, cattleDetailsHistoric.damId);
            SetCell("J" + rowStart, cattleDetailsHistoric.passport);
            SetCell("K" + rowStart, cattleDetailsHistoric.lateType);
            SetCell("L" + rowStart, cattleDetailsHistoric.onOff);
            SetCell("M" + rowStart, cattleDetailsHistoric.moveDate);
            SetCell("N" + rowStart, cattleDetailsHistoric.notificationDate);

            return ++rowStart;
        }

        public void ApplyStats(LateCattleCount lateCattleCount)
        {
            SetCell("I13", lateCattleCount.TotalMovesThisYear);
            SetCell("C19", lateCattleCount.FMXC1Count);
            SetCell("C20", lateCattleCount.FMXC2Count);
            SetCell("C21", lateCattleCount.DDXC1Count);
            SetCell("C22", lateCattleCount.DDXC2Count);
            SetCell("C23", lateCattleCount.LRXC1Count);
            SetCell("C24", lateCattleCount.LRXC2Count);
            SetCell("C25", lateCattleCount.IMPXC1Count);
            SetCell("C26", lateCattleCount.IMPXC2Count);
            SetCell("I21", lateCattleCount.HEA);

        }

    }
}