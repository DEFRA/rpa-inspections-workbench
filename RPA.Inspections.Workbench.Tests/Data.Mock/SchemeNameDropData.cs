using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class SchemeNameDropData
    {
        public static List<SchemeNameDrop> Data()
        {
            return new List<SchemeNameDrop>
            {
                new SchemeNameDrop
                {
                    SchemeNameDropId =1,
                    Active = true,
                    Text = "CII20"
                }
            };
        }
    }
}
