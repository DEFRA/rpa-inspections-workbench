using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class CattleDetailsCondensed
    {
        public string earTag { get; set; }
        public string herdMark { get; set; }
        public string checkDigit { get; set; }
        public string animalNumber { get; set; }
        public string breed { get; set; }
        public string gender { get; set; }
        public string birthDate { get; set; }

        public CattleDetailsCondensed(Cattle cattle, List<string> EarTagBrokenDown)
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
                if (cattle.birthDate.Value.ToShortDateString() != "01/01/1950")
                {
                    birthDate = cattle.birthDate.Value.ToShortDateString();
                }
                else
                {
                    birthDate = "11/11/1111";
                }
            }
        }
    }
}