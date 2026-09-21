using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class CattleData
    {
        public static List<Cattle> Data()
        {
            return new List<Cattle>
            {

                new Cattle {cattleId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), earTag = "TestEarTag", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LIST1", deathDate = null},
                new Cattle {cattleId = Guid.Parse("A462AEEE-537A-4E5D-802D-9B2D6F6F979D"), earTag = "TestEarTag2", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LIST1", deathDate = null},
                new Cattle {cattleId = Guid.Parse("F999406A-2746-491D-A638-724C9021F53D"), earTag = "TestEarTag4", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LIST2", offDate = DateTime.Now.AddMonths(-6), deathDate = DateTime.Now.AddDays(-7)},
                new Cattle {cattleId = Guid.Parse("32EF2C8D-E8BC-4373-BB18-509953D0098C"), earTag = "TestEarTag3", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LIST2", offDate = DateTime.Now.AddMonths(-6), deathDate = DateTime.Now.AddDays(-14)},
                new Cattle {cattleId = Guid.Parse("9C72F235-84B0-4D35-9E60-C8E1CF887B7E"), earTag = "TestEarTag8", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LIST2", offDate = DateTime.Now.AddMonths(-6), deathDate = DateTime.Now.AddYears(-1)},
                new Cattle {cattleId = Guid.Parse("5A5E2C65-7A63-4BDE-B39A-CA1338A54568"), earTag = "TestEarTag9", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LIST2", offDate = DateTime.Now.AddMonths(-6), deathDate = DateTime.Now.AddYears(-1)},
                new Cattle {cattleId = Guid.Parse("F490179F-3AC5-4823-8145-7DCAD018CFFF"), earTag = "TestEarTag10", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LATE", lateNotification = true, notificationDate = DateTime.Now},
                new Cattle {cattleId = Guid.Parse("14969238-C0A9-42FF-92D6-312543B88E82"), earTag = "TestEarTag11", holdingId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), listNumber = "LATE", lateNotification = true, notificationDate = DateTime.Now}, 
            };
        }
    }
}
