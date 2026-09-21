using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels.Functions
{
    public static class EarTag
    {
        public static List<string> EarTagBreakdown(string earTag)
        {
            string safeEarTag = earTag.Replace(" ", String.Empty);
            string endofTag = safeEarTag.Substring((safeEarTag.Length - 2), 2);

            string herdmark = "";
            string checkDigit = "";
            string animalNumber = "";

            if (earTag.Substring(0, 2) == "GB" || earTag.Substring(0, 3) == "826")
            {
                herdmark = earTag.Substring(earTag.Length - 11, 6);
                animalNumber = earTag.Substring(earTag.Length - 5);
            }
            else if (earTag.Length == 14 && endofTag != "oo")
            {

                herdmark = earTag.Substring(0, 8);
                checkDigit = earTag.Substring(8, 1);
                animalNumber = earTag.Substring(9, earTag.Length - 9);

            }

            List<string> tagBreakdown = new List<string> { herdmark, checkDigit, animalNumber };

            return tagBreakdown;
        }
    }
}