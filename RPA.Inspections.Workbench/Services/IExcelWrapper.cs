using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Services
{
    interface IExcelWrapper
    {
        void Open(string fileName);
        object ReadValue(string sheetName, string cell);
        void SetValue(string sheetName, string cell, object value);
    }
}
