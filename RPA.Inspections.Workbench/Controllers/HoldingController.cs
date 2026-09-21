using PagedList;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Controllers
{
    public class HoldingController : Controller
    {
        IPeopleContext Pdb;
        IWorkbenchContext db;

  
        IHoldingService holdingService;
        ILinkedHoldingService linkedHoldingService;
        ICattleService cattleService;
        IPackRequested packRequestedService;
        IFileService fileService;
        IMailService mailService;
        ISendService sendService;
        IFilterService filterService;
        IUserHelper userHelper;
        IHolidayService holidayService;
        public HoldingController()
        {
            db = new WorkbenchContext();
            Pdb = new PeopleContext();
            userHelper = new UserHelper(Pdb);
            filterService = new FilterService(db, Pdb);
            holdingService = new HoldingService(db, Pdb, filterService, userHelper);
            linkedHoldingService = new LinkedHoldingService(db);
            cattleService = new CattleService(db);
            sendService = new SendService();
            mailService = new MailService(db);
            holidayService = new HolidayService();
            packRequestedService = new PackRequestedService(db, mailService, userHelper, sendService);
            fileService = new FileService(db, cattleService, holdingService, sendService, mailService, holidayService);

        }


        public HoldingController(IWorkbenchContext db, IPeopleContext Pdb, IHoldingService holdingService, ILinkedHoldingService linkedHoldingService, ICattleService cattleService, IPackRequested packRequestedService, IMailService mailService, IFileService fileService, IFilterService filterService, IUserHelper userHelper, ISendService sendService, IHolidayService holidayService)
        {
            this.db = db;
            this.Pdb = Pdb;
            this.holdingService = holdingService;
            this.linkedHoldingService = linkedHoldingService;
            this.cattleService = cattleService;
            this.packRequestedService = packRequestedService;
            this.mailService = mailService;
            this.sendService = sendService;
            this.fileService = fileService;
            this.filterService = filterService;
            this.userHelper = userHelper;
            this.holidayService = holidayService;

        }


        // GET: Holding
        public ActionResult Index(string searchstring = null, int page = 1, int pageSize = 10)

        {

                PagedList<Holding> model;

                if (!string.IsNullOrEmpty(searchstring))
                {
                    model = holdingService.GetHolding(searchstring, page, pageSize);

                    if (model.Count == 0)
                    {
                        TempData["InvalidCPH"] = "Invalid CPH Entered or CPH not Found";
                    }
                }
                else
                {
                    model = holdingService.GetHolding(null, page, pageSize);
                }

                ViewBag.searchData = searchstring;
                ViewBag.LoggedInUser = userHelper.CurrentUser();
                return View(model);

        }

        //GET: All Packs Requested
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult PacksRequested()
        {

                var model = packRequestedService.GetOutstandingPackRequests();

                return View(model);
            

        }

        //GET: All Generated Packs for user and display in partial view
        public ActionResult _GeneratedPacks()
        {

                var model = packRequestedService.GetDownloadableRequests();

                return PartialView(model);
            

        }


        //GET: Display Holding Overview

        public ActionResult ViewHolding(Guid holdingId)
        {

                string userid = userHelper.CurrentUser();

                Holding model = holdingService.GetHoldingById(holdingId);

                ViewBag.IDHolding = holdingId;

                ViewBag.userMail = userHelper.CurrentUserEmail(userid);

                return View(model);

        }

        //GET: Display Linked Holdings in Partial View

        public ActionResult _ViewLinkedHoldings(Guid holdingId)
        {

                List<LinkedHolding> model = linkedHoldingService.GetLinkedholdingById(holdingId);

                ViewBag.linkedIdHolding = holdingId;

                return PartialView(model);

        }

        //GET: Display Cattle in Partial View

        public ActionResult _ViewCattle(Guid holdingId)
        {

                List<Cattle> model = cattleService.GetCattleByHoldingId(holdingId);

                ViewBag.cattleIdholding = holdingId;

                return PartialView(model);

        }

        //GET: Display Late Cattle in Partial View

        public ActionResult _LateCattle(Guid holdingId)
        {

                List<Cattle> model = cattleService.GetLateCattle(holdingId);

                ViewBag.cattleIdholding = holdingId;

                return PartialView(model);

        }

        //GET: Display Cattle Count in Partial View

        public ActionResult _CountCattle(Guid holdingId)
        {

                List<Cattle> model = cattleService.GetCattleByHoldingId(holdingId);

                DateTime offDateTest = DateTime.Now.AddYears(-1);
                DateTime offDeathTest = DateTime.Now.AddDays(-28);

                ViewBag.cattleIdholding = holdingId;

                ViewBag.List1 = model.Where(x => x.listNumber == "LIST1").Count();

                ViewBag.List2 = model.Where(x => x.listNumber == "LIST2").Count();

                ViewBag.Other = model.Where(x => x.listNumber == "").Count();

                return PartialView(model);

        }

        //GET: Display Pack Requested in Partial View

        public ActionResult _ViewPackRequested(Guid holdingId)
        {

                PackRequested model = packRequestedService.GetPackRequestedById(holdingId);

                ViewBag.PackReqHoldingId = holdingId;

                return PartialView(model);

        }

        //GET: Test Methods - ****For Testing Purposes Only****

        //public ActionResult TestMethods(Guid? holdingId)
        //{

        //    if (holdingId == null)
        //    {
        //        holdingId = Guid.Parse("FA11ACD9-F89E-4893-8565-DB792CBB2731");
        //    }

        //    fileService.RunAllBelow(holdingId.Value);

        //    //return Content("");

        //    return RedirectToAction("PacksRequested");
        //}

        //POST - Pack Requested
        [HttpPost]        
        public ActionResult PackRequest(Guid holdingId, string cph)
        {

                packRequestedService.CreatePackRequested(holdingId, cph);

                db.SaveChanges();

                return Content("");
        }


        //Update the People database with user's new email
        [HttpPost]
        public ActionResult PeopleUpdate(Guid holdingId, string UserEmail)
        {

                string userId = userHelper.CurrentUser();

                var query = from ppl in Pdb.People
                            where ppl.StaffNumber == userId
                            select ppl;


                foreach (Person people in query)
                {
                    people.Email = UserEmail;
                }



                db.SaveChanges();

                return Content("");

        }

        //Code to Get the Pack

        public FileResult GetPack(string savePath)
        {

                byte[] packFile = fileService.StreamFile(savePath);

                FileInfo info = new FileInfo(savePath);

                return File(packFile, System.Net.Mime.MediaTypeNames.Application.Octet, info.Name);

        }

        //POST: Delete Specific pack

        public ActionResult DeletePack(string savePath, Guid packID)
        {

                packRequestedService.DeletePack(savePath, packID);

                return RedirectToAction("PacksRequested");

        }


    }


}