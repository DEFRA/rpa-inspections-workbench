using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Controllers
{
    [Authorize(Roles = "CI Inspections Workbench: BCMS Sev Calc Generator")]

    public class BcmsAdminController : Controller
    {
        IWorkbenchContext db;


        public BcmsAdminController()
        {
           db = new WorkbenchContext();
        }

        public BcmsAdminController(IWorkbenchContext db)
        {
            this.db = db;
        }

        //Get: Bcms Admin page
        public ActionResult Index()
        {
            List<BcmsRequested> bcmsRequestedList = db.BcmsRequested.OrderByDescending(x => x.DatePackRequested).Take(10).ToList();

            return View(bcmsRequestedList);
        }

        [HttpPost]
        //Bulk Request
        public ActionResult BulkRequest(DateTime startDate, DateTime endDate)
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
        public ActionResult SpecificRequest(string cph, int schemeYear)
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

        public ActionResult open_report(string name)
        {
            
            return new FilePathResult(name, "text/html");

        }
    }
}