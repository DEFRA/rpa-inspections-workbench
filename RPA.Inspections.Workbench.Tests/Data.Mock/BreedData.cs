using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class BreedData
    {
        public static List<Breed> Data()
        {
            return new List<Breed>
            {
                new Breed {breedId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), breedCode = "MCW", breedFull = "Mega Cow", Active = true},
                new Breed {breedId = Guid.Parse("A462AEEE-537A-4E5D-802D-9B2D6F6F979D"), breedCode = "MCW1", breedFull = "Mega Cow 1", Active = true },
                new Breed {breedId = Guid.Parse("F999406A-2746-491D-A638-724C9021F53D"), breedCode = "MCW2", breedFull = "Mega Cow 2", Active = true},
                new Breed {breedId = Guid.Parse("32EF2C8D-E8BC-4373-BB18-509953D0098C"), breedCode = "MCW3", breedFull = "Mega Cow 3", Active = true}
            };
        }
    }
}
