using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{

    public class CattleDetailsHistoric
    {
        IHolidayService holidayService;
        public string earTag { get; set; }
        public string breed { get; set; }
        public string gender { get; set; }
        public string birthDate { get; set; }
        public string onDate { get; set; }
        public string offDate { get; set; }
        public string deathDate { get; set; }
        public string damId { get; set; }
        public string passport { get; set; }
        public string onOff { get; set; }
        public string moveDate { get; set; }
        public string notificationDate { get; set; }
        public string lateType { get; set; }


        public CattleDetailsHistoric(Cattle cattle, DateTime notifiedDate, DateTime lateDate, IHolidayService holidayService)
        {

            double days = holidayService.GetWorkingDays(lateDate, notifiedDate);

            earTag = cattle.earTag;

            if (cattle.Breed != null)
            {
                breed = cattle.Breed.breedFull;
            }
            else
            {
                breed = "UNKNOWN";
            }
            gender = cattle.gender;
            if (cattle.birthDate.HasValue)
            {
                if (cattle.birthDate.Value.ToShortDateString() != "01/01/1950")
                {
                    birthDate = cattle.birthDate.Value.ToShortDateString();
                }
                else
                {
                    birthDate = "11/11/1111";
                }
            }
            if (cattle.onDate.HasValue)
            {
                if (cattle.onDate.Value.ToShortDateString() != "01/01/1950")
                {
                    onDate = cattle.onDate.Value.ToShortDateString();
                }
                else
                {
                    onDate = "11/11/1111";
                }
            }
            if (cattle.offDate.HasValue)
            {
                if (cattle.offDate.Value.ToShortDateString() != "01/01/1950")
                {
                    offDate = cattle.offDate.Value.ToShortDateString();
                }
                else
                {
                    offDate = "11/11/1111";
                }
            }
            if (cattle.deathDate.HasValue)
            {
                if (cattle.deathDate.Value.ToShortDateString() != "01/01/1950")
                {
                    deathDate = cattle.deathDate.Value.ToShortDateString();
                }
                else
                {
                    deathDate = "11/11/1111";
                }
            }
            damId = cattle.damId;
            passport = cattle.passportVersion;




            switch (cattle.lateType)
            {

                case "LATE_ON":

                    if (days > 7 && days < 22)
                    {
                        lateType = "FMXC 1";
                    }
                    else if (days > 21)
                    {
                        lateType = "FMXC 2";
                    }

                    break;

                case "LATE_OFF":

                    if (days > 7 && days < 22)
                    {
                        lateType = "FMXC 1";
                    }
                    else if (days > 21)
                    {
                        lateType = "FMXC 2";
                    }

                    break;

                case "LATE_DEATH":

                    TimeSpan timeSpanA = notifiedDate - lateDate;

                    days = Math.Round(timeSpanA.TotalDays);

                    if (days > 7 && days < 22)
                    {
                        lateType = "DDXC 1";
                    }
                    else if (days > 21)
                    {
                        lateType = "DDXC 2";
                    }

                    break;

                case "LATE_BIRTH":

                    TimeSpan timeSpanB = notifiedDate - lateDate;

                    days = Math.Round(timeSpanB.TotalDays);

                    if (days > 27 && days < 42)
                    {
                        lateType = "LRXC 1";
                    }
                    else if (days > 41)
                    {
                        lateType = "LRXC 2";
                    }

                    break;

                case "LATE_IMPORT":

                    TimeSpan timeSpanC = notifiedDate - lateDate;

                    days = Math.Round(timeSpanC.TotalDays);

                    if (days > 15 && days < 30)
                    {
                        lateType = "IMPXC 1";

                    }
                    else if (days > 29)
                    {
                        lateType = "IMPXC 2";
                    }

                    break;
                default:
                    lateType = "NOT KNOWN";
                    break;
            }



            if (cattle.lateType == "LATE_ON")
            {
                onOff = "On";
            }
            else if (cattle.lateType == "LATE_OFF")
            {
                onOff = "Off";
            }
            moveDate = cattle.lateDate.Value.ToShortDateString();
            notificationDate = cattle.notificationDate.Value.ToShortDateString();
        }
    }
}