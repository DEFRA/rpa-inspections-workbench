using PagedList;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public class BreedService : IBreedService
    {

        IWorkbenchContext db;

        public BreedService(IWorkbenchContext context)
        {
            this.db = context;
        }

        //create a paged list of all existing breeds

        public PagedList<Breed> GetBreeds(string searchString, int page, int pageSize)
        {
            List<Breed> model;

            if (string.IsNullOrEmpty(searchString))
            {
                model = db.Breed.OrderBy(x => x.breedCode).ToList();
            }
            else
            {
                model = db.Breed.Where(x => x.breedCode.ToUpper().Contains(searchString.ToUpper().Trim()) || x.breedFull.ToUpper().Contains(searchString.ToUpper().Trim())).OrderBy(x => x.breedCode).ToList();
            }

            PagedList<Breed> pModel = new PagedList<Breed>(model, page, pageSize);

            return pModel;
        }

        //get a specific breed by id

        public Breed GetBreedById(Guid? breedId)
        {
            Breed model = db.Breed.Where(x => x.breedId == breedId).FirstOrDefault();

            return model;
        }




    }
}