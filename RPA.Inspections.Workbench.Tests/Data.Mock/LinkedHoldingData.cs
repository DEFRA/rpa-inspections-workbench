using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class LinkedHoldingData
    {
        public static List<LinkedHolding> Data()
        {
            return new List<LinkedHolding>
            {
             new LinkedHolding {holdingId = Guid.Parse("D0F53C5B-9717-48B1-A40F-0044003DDC14"), CPHNumberLH = "01/011/1004", isLink = true },
            };
        }
    }
}
