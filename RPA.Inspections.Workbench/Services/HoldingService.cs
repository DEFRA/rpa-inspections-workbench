using PagedList;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Extensions;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public class HoldingService : IHoldingService
    {

        IWorkbenchContext db;
        IPeopleContext Pdb;
        IFilterService filterService;
        IUserHelper userHelper;
 


        public HoldingService(IWorkbenchContext context, IPeopleContext Pcontext, IFilterService filterService, IUserHelper userHelper)
        {
            this.db = context;
            this.Pdb = Pcontext;
            this.filterService = filterService;
            this.userHelper = userHelper;
        }


        public PagedList<Holding> GetHolding(string searchString, int page, int pageSize)
        {
            //create list of all Active holding's

            var holdingList = filterService.Filterholdings();
            holdingList = holdingList.Where(x => x.AssignedUser != null).ToList();

            List<Holding> model = new List<Holding>();
            IEnumerable<Holding> holdingsAssigned = GetAllHoldingsForCurrentUser(holdingList);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();

                string newInput = Regex.Replace(searchString, "[^0-9a-zA-Z]+", "");

                foreach (var holding in holdingsAssigned)
                {
                    string cphNumber = Regex.Replace(holding.cphNumber, "[^0-9a-zA-Z]+", "");


                    if (cphNumber != null && cphNumber == newInput)
                    {
                        model.Add(holding);
                    }
                }

                model = model.OrderBy(x => x.primaryName).ToList();
            }
            else
            {

                foreach (var holding in holdingsAssigned)
                {
                        model.Add(holding);
                }

                model = model.OrderBy(x => x.primaryName).ToList();
            }

            PagedList<Holding> pagedListHoldings = new PagedList<Holding>(model, page, pageSize);

            return pagedListHoldings;
        }

        private IEnumerable<Holding> GetAllHoldingsForCurrentUser(List<Holding> holdingList)
        {
            var allStaffNumbers = userHelper.GetAllStaffNumbersForCurrentUser();

            var filteredHoldings = from holding in holdingList
                                   where allStaffNumbers.Any(staffNumber => staffNumber.Equals(holding.AssignedUser, StringComparison.OrdinalIgnoreCase)) && holding.Active
                                   select holding;

            return filteredHoldings.ToList();
        }

        public PagedList<Holding> GetAllHolding(string searchString, int page, int pageSize)
        {
            //create list of all  holding's

            List<Holding> mainHoldingList = filterService.Filterholdings();

            List<Holding> filteredHoldingList = new List<Holding>();

            if (string.IsNullOrEmpty(searchString))
            {
                filteredHoldingList = mainHoldingList.OrderBy(x => x.primaryName).ToList();
            }
            else
            {
                string newInput = searchString.Trim();

                foreach (var holding in mainHoldingList)
                {
                    string cphNumber = holding.cphNumber.ToString();


                    if (cphNumber != null && cphNumber.Contains(newInput) || (holding.primaryName != null && holding.primaryName.ToUpper().Contains(searchString.ToUpper().Trim())) || (holding.AssignedUser != null && holding.AssignedUser.ToUpper().Contains(searchString.ToUpper().Trim())))
                    {
                        filteredHoldingList.Add(holding);
                    }
                }

                filteredHoldingList = filteredHoldingList.OrderBy(x => x.primaryName).ToList();


            }

            PagedList<Holding> pModel = new PagedList<Holding>(filteredHoldingList, page, pageSize);

            return pModel;
        }

        //Get a holding by ID

        public Holding GetHoldingById(Guid? HoldingId)
        {

            var holdingList = filterService.Filterholdings();

            Holding model = holdingList.Where(x => x.holdingId == HoldingId).FirstOrDefault();

            return model;
        }


        public Holding GetArchivedHoldingById(Guid? HoldingId)
        {
            Holding holding = db.Holding.Where(x => x.holdingId == HoldingId).FirstOrDefault();

            return holding;
        }

        //Get using a CPH

        public Holding GetHoldingByCPH(string CPH)
        {

            var holdingList = filterService.Filterholdings();

            Holding model = holdingList.Where(x => x.cphNumber == CPH).FirstOrDefault();

            return model;
        
        }

        public List<Person> PersonList(string searchstring)
        
        // Create a list of People
        {

            searchstring = searchstring.ToUpper();

            List<Inspector> inspList = db.Inspector.ToList();
            List<Person> peopleList = Pdb.People.OrderBy(x => x.Name).ToList();

            peopleList = peopleList.Where(x => inspList.Exists(p => p.staffNumber.ToUpper() == x.StaffNumber.ToUpper())).OrderBy(x => x.Name).ToList();
                           

            if (!string.IsNullOrEmpty(searchstring))
            {
                peopleList = peopleList.Where(x => x.Name.ToUpper().Contains(searchstring) || x.StaffNumber.ToUpper().Contains(searchstring)).OrderBy(x => x.Name).ToList();

            }

            return peopleList;
        }


        public List<Holding> GetAllArchivedHoldings(string searchString, List<string> archivedYears, string searchYear)
        {
            //create list of all  holding's

            List<Holding> archivedHoldings = new List<Holding>();

            foreach(string year in archivedYears)
            {
                foreach(Holding holding in db.Holding)
                {
                    if(holding.schemeYear == year)
                    {
                        archivedHoldings.Add(holding);
                    }
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToUpper();
                archivedHoldings = archivedHoldings.Where(x => x.cphNumber.ToUpper().Contains(searchString) || x.primaryName.ToUpper().Contains(searchString) || x.AssignedUser.ToUpper().Contains(searchString)).ToList();
            }
            else if(!string.IsNullOrEmpty(searchYear))
            {
                archivedHoldings = archivedHoldings.Where(x => x.schemeYear == searchYear).ToList();
            }

            return archivedHoldings;
        }
    }
}