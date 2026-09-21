using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class CattleDetails
    {
        public string earTag { get; set; }
        public string herdMark { get; set; }
        public string checkDigit { get; set; }
        public string animalNumber { get; set; }
        public string breed { get; set; }
        public string gender { get; set; }
        public string birthDate { get; set; }
        public string onDate { get; set; }
        public string offDate { get; set; }
        public string deathDate { get; set; }
        public string damId { get; set; }
        public string passport { get; set; }

        public CattleDetails(Cattle cattle, List<string> EarTagBrokenDown)
        {
            earTag = cattle.earTag;
            herdMark = EarTagBrokenDown[0];
            checkDigit = EarTagBrokenDown[1];
            animalNumber = EarTagBrokenDown[2];

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
                if (cattle.birthDate.Value.ToShortDateString() != new DateTime(1950, 1, 1).ToShortDateString())
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
                if (cattle.onDate.Value.ToShortDateString() != new DateTime(1950, 1, 1).ToShortDateString())
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
                if (cattle.offDate.Value.ToShortDateString() != new DateTime(1950, 1, 1).ToShortDateString())
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
                if (cattle.deathDate.Value.ToShortDateString() != new DateTime(1950, 1, 1).ToShortDateString())
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
        }
    }
}