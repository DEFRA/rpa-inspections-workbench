using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class SheetModelBase
    {
        protected ExcelRange cells;

        public SheetModelBase(ExcelRange cells)
        {
            this.cells = cells;
        }

        public virtual void SetCell(string key, object value)
        {
            cells[key].Value = value;

        }
    }
}