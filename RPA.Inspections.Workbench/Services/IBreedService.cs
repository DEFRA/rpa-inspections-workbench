using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;

namespace RPA.Inspections.Workbench.SL
{
    public interface IBreedService
    {
        PagedList<Breed> GetBreeds(string searchString, int page, int pageSize);

        Breed GetBreedById(Guid? breedId);
    }
}
