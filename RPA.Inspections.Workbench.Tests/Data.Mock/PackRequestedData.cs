using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class PackRequestedData
    {
        public static List<PackRequested> Data()
        {
            return new List<PackRequested>
            {
            new PackRequested { packRequestedId = Guid.Parse("0C51DC76-B92D-420A-BA5F-006E564C06ED"), holdingId = Guid.Parse("214EC889-7EC8-46AD-AFA8-0030607A34EF"), activeRequest = false, requestedBy = "m600300", packGenerated = DateTime.UtcNow, display = true },
            new PackRequested { packRequestedId = Guid.Parse("826AAE4E-011E-4B6F-A593-007388811AB0"), holdingId = Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2"), activeRequest = true, requestedBy = "m600300", packGenerated = DateTime.UtcNow, display = false },
            new PackRequested { packRequestedId = Guid.Parse("A4609495-1FA5-4BEB-9B09-00777D219F31"), holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), activeRequest = false, requestedBy = "m600300", packGenerated = DateTime.UtcNow, display = true, requestorEmail = "test@test.com" },
            new PackRequested { packRequestedId = Guid.Parse("79419019-A2CF-4218-A18C-00D3BD60C671"), holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), activeRequest = false, requestedBy = "m600300", packGenerated = DateTime.UtcNow, display = true },
            };
        }
    }
}
