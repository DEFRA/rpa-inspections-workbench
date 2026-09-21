using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class InspectorData
    {
        public static List<Inspector> Data()
        {
            return new List<Inspector>
            {
                new Inspector
                {
                    inspectorId = Guid.Parse("C5F36F2D-C531-474C-8688-01831DCF1F0E"),
                    staffNumber = "M600300"
                }
                
            };
        }
    }
}
