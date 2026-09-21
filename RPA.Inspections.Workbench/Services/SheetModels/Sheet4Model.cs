using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet4Model : SheetModelBase
    {
        public int listOneCount { get; set; }
        public int listTwoCount { get; set; }

        public Sheet4Model(List<Cattle> cattleList, ExcelRange cells) : base(cells)
        {
            listOneCount = cattleList.Count(x => x.listNumber == "LIST1");
            listTwoCount = cattleList.Count(x => x.listNumber == "LIST2");
        }

        public void Apply()
        {
            SetCell("B3", listOneCount);
            SetCell("B5", listTwoCount);
        }
    }
}