using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class HoldingData
    {
        public static List<Holding> Data()
        {
            return new List<Holding>
            {
                new Holding {holdingId = Guid.Parse("9548D01E-15FC-4CB5-97AB-000EB6EFD6C3"), cphNumber = "01/011/1001", Active = true, primaryName = "Big Farm", AssignedUser = "M600300", schemeYear = "2019", SchemeName = "CII19", SchemeNameDropId = 1 },
                new Holding {holdingId = Guid.Parse("214EC889-7EC8-46AD-AFA8-0030607A34EF"), cphNumber = "01/011/1002", Active = true, primaryName = "Big Farm1", AssignedUser = "M600300", schemeYear = "2020" , SchemeName = "CII20" },
                new Holding {holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), cphNumber = "01/011/1003", Active = true, primaryName = "Big Farm2", AssignedUser = "M600300", schemeYear = "2019" , SchemeName = "CII19" },
                new Holding {holdingId = Guid.Parse("D0F53C5B-9717-48B1-A40F-0044003DDC14"), cphNumber = "01/011/1004", Active = true, primaryName = "Big Farm3", AssignedUser = "M600300", schemeYear = "2017" , SchemeName = "CII18" },
                new Holding {holdingId = Guid.Parse("6CD26670-B5F3-4EAE-B8BE-00444D6A04C3"), cphNumber = "01/011/1005", Active = true, primaryName = "Archived Farm", AssignedUser = "M600300", schemeYear = "2017" , SchemeName = "CII17" },
                new Holding {holdingId = Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2"), cphNumber = "01/011/1010", Active = true, primaryName = "Hairy Farm", AssignedUser = "M600300", schemeYear = "2020" , SchemeName = "CII20" },
            };
        }
    }
}
