using Microsoft.AspNetCore.Authorization;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace RPA.Inspections.Workbench.Controllers
{
    [Authorize(Roles = "CI Inspections Workbench: BCMS Sev Calc Generator")]

    public class BcmsAdminController : Controller
    {
        IWorkbenchContext db;


        public BcmsAdminController(IWorkbenchContext db)
        {
            this.db = db;
        }

        //Get: Bcms Admin page
        public IActionResult Index()
        {
            List<BcmsRequested> bcmsRequestedList = db.BcmsRequested.OrderByDescending(x => x.DatePackRequested).Take(10).ToList();

            return View(bcmsRequestedList);
        }

        [HttpPost]
        //Bulk Request
        public IActionResult BulkRequest(DateTime startDate, DateTime endDate)
        {
            BcmsRequested bcmsRequested = new BcmsRequested();

            bcmsRequested.BcmsPackRequestedId = Guid.NewGuid();
            bcmsRequested.StartDate = startDate;
            bcmsRequested.EndDate = endDate;
            bcmsRequested.ActiveRequest = true;
            bcmsRequested.DatePackRequested = DateTime.Now;
            bcmsRequested.BulkRequest = true;
            bcmsRequested.SchemeYear = null;

            db.BcmsRequested.Add(bcmsRequested);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        //Spec Request
        public IActionResult SpecificRequest(string cph, int schemeYear)
        {
            BcmsRequested bcmsRequested = new BcmsRequested();

            bcmsRequested.BcmsPackRequestedId = Guid.NewGuid();
            bcmsRequested.StartDate = null;
            bcmsRequested.EndDate = null;
            bcmsRequested.CphNumber = cph;
            bcmsRequested.ActiveRequest = true;
            bcmsRequested.DatePackRequested = DateTime.Now;
            bcmsRequested.SchemeYear = schemeYear;
            bcmsRequested.Filepath = null;

            db.BcmsRequested.Add(bcmsRequested);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult open_report(string name)
        {
            
            // Only reports that were generated for a BCMS request can be opened
            if (!db.BcmsRequested.Any(x => x.Filepath == name))
            {
                return NotFound();
            }

            return PhysicalFile(name, "text/html");

        }
    }
}