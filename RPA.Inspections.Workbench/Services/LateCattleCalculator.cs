using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class LateCattleCalculator
    {

        public LateCattleCount Calculator(Cattle cattle, LateCattleCount lateCattleCount)
        {
            DateTime notificationDate = cattle.notificationDate.HasValue ? cattle.notificationDate.Value : DateTime.Now;
            DateTime lateDate = cattle.lateDate.HasValue ? cattle.lateDate.Value : DateTime.Now;

            switch (cattle.lateType)
            {


                case "LATE_ON":

                    double days = HolidayService.GetWorkingDays(lateDate, notificationDate);

                    if (days > 7 && days < 22)
                    {
                        lateCattleCount.FMXC1Count = lateCattleCount.FMXC1Count + 1;
                    }
                    else if (days > 21)
                    {
                        lateCattleCount.FMXC2Count = lateCattleCount.FMXC2Count + 1;
                    }

                    break;

                case "LATE_OFF":


                    days = HolidayService.GetWorkingDays(lateDate, notificationDate);

                    if (days > 7 && days < 22)
                    {
                        lateCattleCount.FMXC1Count = lateCattleCount.FMXC1Count + 1;
                    }
                    else if (days > 21)
                    {
                        lateCattleCount.FMXC2Count = lateCattleCount.FMXC2Count + 1;
                    }

                    break;

                case "LATE_DEATH":

                    TimeSpan timeSpanA = notificationDate - lateDate;

                    days = Math.Round(timeSpanA.TotalDays);

                    if (days > 7 && days < 22)
                    {
                        lateCattleCount.DDXC1Count = lateCattleCount.DDXC1Count + 1;
                    }
                    else if (days > 21)
                    {
                        lateCattleCount.DDXC2Count = lateCattleCount.DDXC2Count + 1;
                    }

                    break;

                case "LATE_BIRTH":

                    TimeSpan timeSpanB = notificationDate - lateDate;

                    days = Math.Round(timeSpanB.TotalDays);

                    if (days > 27 && days < 42)
                    {
                        lateCattleCount.LRXC1Count = lateCattleCount.LRXC1Count + 1;
                    }
                    else if (days > 41)
                    {
                        lateCattleCount.LRXC2Count = lateCattleCount.LRXC2Count + 1;
                    }

                    break;

                case "LATE_IMPORT":

                    TimeSpan timeSpanC = notificationDate - lateDate;

                    days = Math.Round(timeSpanC.TotalDays);

                    if (days > 15 && days < 30)
                    {
                        lateCattleCount.IMPXC1Count = lateCattleCount.IMPXC1Count + 1;
                    }
                    else if (days > 29)
                    {
                        lateCattleCount.IMPXC2Count = lateCattleCount.IMPXC2Count + 1;
                    }

                    break;
                default:
                    break;
            }

            return lateCattleCount;
        }
    }
}