using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.Functions
{
    public static class TenPercentCattleSelector
    {
        public static List<Cattle> CattleSelector(int selectionNumber, List<Cattle> cattleList, int listTwoCount)
        {

            int tenPercentListTwo = (int)Math.Ceiling(listTwoCount * 0.1);
            List<Cattle> cattleSelection = new List<Cattle>();

            switch (selectionNumber)
            {
                case 1:
                    cattleSelection = cattleList.FindAll(x => x.listNumber == "LIST2").Take(tenPercentListTwo).ToList();
                    break;
                case 2:
                    cattleSelection = cattleList.FindAll(x => x.listNumber == "LIST2").Skip(tenPercentListTwo).Take(tenPercentListTwo).ToList();
                    break;
                case 3:
                    cattleSelection = cattleList.FindAll(x => x.listNumber == "LIST2").Skip(tenPercentListTwo * 2).ToList();
                    break;
            }

            return cattleSelection;
        }
    }
}