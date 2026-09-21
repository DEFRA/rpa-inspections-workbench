using PagedList;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Controllers
{
    [Authorize(Roles = "CI Inspections Workbench: Admin User")]

    public class AdminController : Controller
    {

        IWorkbenchContext db;
        IPeopleContext Pdb;
               
        IHoldingService holdingService;
        IBreedService breedService;
        IFileService fileService;
        IMailService mailService;
        ICattleService cattleService;
        ISendService sendService;
        IPackRequested packService;
        IFilterService filterService;
        IUserHelper userHelper;
        IHolidayService holidayService;


        public AdminController()
        {

            db = new WorkbenchContext();
            Pdb = new PeopleContext();
            userHelper = new UserHelper(Pdb);
            filterService = new FilterService(db, Pdb);
            holdingService = new HoldingService(db, Pdb, filterService, userHelper);
            cattleService = new CattleService(db);
            sendService = new SendService();
            breedService = new BreedService(db);
            mailService = new MailService(db);
            packService = new PackRequestedService(db, mailService, userHelper, sendService);
            holidayService = new HolidayService();
            fileService = new FileService(db, cattleService, holdingService, sendService, mailService, holidayService);
        }

        public AdminController(IWorkbenchContext db, IPeopleContext Pdb, IHoldingService holdingService, ICattleService cattleService, IBreedService breedService, IPackRequested packService, IMailService mailService, IFileService fileService, IFilterService filterService, IUserHelper userHelper, ISendService sendService, IHolidayService holidayService)
        {
            this.db = db;
            this.Pdb = Pdb;
            this.filterService = filterService;
            this.holdingService = holdingService;
            this.cattleService = cattleService;
            this.breedService = breedService;
            this.packService = packService;
            this.mailService = mailService;
            this.fileService = fileService;
            this.userHelper = userHelper;
            this.sendService = sendService;
            this.holidayService = holidayService;
        }

        // GET: Admin Main Page
        public ActionResult Index()
        {
            return View();
        }

        //GET: List of all breeds

        public ActionResult BreedList(string searchstring = null, int page = 1, int pageSize = 50)

        {

                var model = breedService.GetBreeds(searchstring, page, pageSize);

                ViewBag.searchData = searchstring;

                return View(model);

        }

        //GET: add a new breed

        public ActionResult AddBreed()
        {

                Breed model = new Breed();

                return View(model);

        }

        //GET: view specific breed to edit

        public ActionResult ViewBreed(Guid breedId)
        {

                Breed model = breedService.GetBreedById(breedId);

                ViewBag.IDbreed = breedId;

                return View(model);

        }


        //GET: List of all holdings

        public ActionResult HoldingList(string searchstring = null, int page = 1, int pageSize = 50)
        {

                var model = holdingService.GetAllHolding(searchstring, page, pageSize);

                ViewBag.searchData = searchstring;

                return View(model);
        }

        //GET: add a new holding

        public ActionResult AddHolding()
        {

                Holding holding = new Holding();

                SetViewData(holding);

                return View(holding);

        }

        //GET: view specific holding to edit

        public ActionResult ViewHolding(Guid HoldingId)
        {


                Holding model = holdingService.GetHoldingById(HoldingId);

                ViewBag.IDHolding = HoldingId;

                SetViewData(model);

                return View(model);

        }

        public ActionResult ArchiveAdmin()
        {

            return View();
        }

        //GET: List of all archived holdings

        public ActionResult ArchivedList(string searchstring = null, string searchYear = null)
        {
                List<string> archivedYears = db.Control.Where(x => x.Property == "SchemeYr" && x.Active == false).OrderBy(c => c.Value).Select(y => y.Value).ToList();

                var model = holdingService.GetAllArchivedHoldings(searchstring, archivedYears, searchYear);

                ViewBag.searchData = searchstring;
                ViewBag.archivedYears = archivedYears;

                return View(model);

        }

        //GET: view specific holding

        public ActionResult ViewArchivedHolding(Guid HoldingId)
        {


                Holding model = holdingService.GetArchivedHoldingById(HoldingId);

                ViewBag.IDHolding = HoldingId;

                return View(model);
        }

        public ActionResult InspectorList()
        {
            List <Inspector> inspectorList = db.Inspector.ToList();

            return View(inspectorList);
        }

        public ActionResult AddInspector()
        {

            Inspector inspector = new Inspector();
                        
            return View(inspector);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddInspector(Inspector inspector)
        {
            if (ModelState.IsValid)
            {
                int exists = db.Inspector.Where(x => x.staffNumber == inspector.staffNumber).Count();

                if (exists == 0)
                {
                    db.Inspector.Add(inspector);

                    db.SaveChanges();
                }

                return RedirectToAction("InspectorList", "Admin");
            }
            else
            {
                return View(inspector);
            }
        }

        public ActionResult RemoveInspector(Guid inspectorId)
        {

            Inspector inspector = db.Inspector.Where(x => x.inspectorId == inspectorId).FirstOrDefault();

            db.Inspector.Remove(inspector);

            db.SaveChanges();

            return RedirectToAction("InspectorList", "Admin");

        }

        //POST: Save Changes to Breed (create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBreed(Breed breed)
        {

            db.Breed.Add(breed);

            db.SaveChanges();

            return RedirectToAction("BreedList", "Admin");

        }

        //POST: Save Changes to Breed (edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewBreed(Breed breed)
        {

                if (ModelState.IsValid)
                {
                    db.SetModified(breed);

                    db.SaveChanges();

                    return RedirectToAction("BreedList", "Admin");
                }

            return View(breed);
        }

        [Authorize(Roles = "CI Inspections Workbench: Super User")]

        //POST: Add a New Inspection List

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HttpPostedFileBase bulkSource, bool deleteAll)
        {
            try
            {

                fileService.RunAddList(bulkSource, deleteAll);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;

                return View("FileLoadFail");
            }
        }

        //POST: Save Changes to Holding (create)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddHolding(Holding holding)
        {
            if (ModelState.IsValid)
            {

                    SchemeNameDrop schemeNameDrop = db.SchemeNameDrop.Where(x => x.SchemeNameDropId == holding.SchemeNameDropId).FirstOrDefault();

                    string schemeYear = "20" + schemeNameDrop.Text.Substring(schemeNameDrop.Text.Length - 2);

                    holding.schemeYear = schemeYear;

                    holding.SchemeName = schemeNameDrop.Text;

                    db.Holding.Add(holding);

                    db.SaveChanges();

                    return RedirectToAction("HoldingList", "Admin");
            }
            else
            {
                SetViewData(holding);

                return View(holding);
            }
        }

        //POST Save Changes to Holding (edit)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewHolding(Holding holding)
        {


            if (holding.AssignedUser != null)
            {
                holding.AssignedUser = holding.AssignedUser.ToUpper();

                holding = ValidateInspector(holding);
            }

            if (ModelState.IsValid)
            {

                    SchemeNameDrop schemeNameDrop = db.SchemeNameDrop.Where(x => x.SchemeNameDropId == holding.SchemeNameDropId).FirstOrDefault();

                    string schemeYear = "20" + schemeNameDrop.Text.Substring(schemeNameDrop.Text.Length - 2);

                    holding.schemeYear = schemeYear;

                    holding.SchemeName = schemeNameDrop.Text;

                    db.SetModified(holding);

                    db.SaveChanges();

                    return RedirectToAction("HoldingList", "Admin");

            }

            SetViewData(holding);

            return View(holding);

        }

        public ActionResult SchemeYearAdmin()
        {
            List<Control> SchemeYears = db.Control.Where(x => x.Property == "SchemeYr").OrderByDescending(p => p.Value).ToList();

            return View(SchemeYears);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SchemeYearAdmin(List<Control> controls)
        {
            foreach (var ctrl in controls)
            {
                string year = ctrl.Value.Substring(ctrl.Value.Length - 2);

                List<SchemeNameDrop> schemeNames = db.SchemeNameDrop.Where(x => x.Text.Substring(x.Text.Length - 2) == year).ToList();

                foreach(var name in schemeNames)
                {
                    name.Active = ctrl.Active;
                    db.SetModified(name);
                }

                db.SetModified(ctrl);
            }

            db.SaveChanges();

            return RedirectToAction("SchemeYearAdmin", "Admin");
        }


        public ActionResult AddSchemeYear()
        {
            string lastSchemeYear = db.Control.Where(x => x.Property == "SchemeYr").OrderByDescending(p => p.Value).Select(c => c.Value).FirstOrDefault();

            int SchemeYearInt = int.Parse(lastSchemeYear) + 1;

            Control newYear = new Control
            {
                Property = "SchemeYr",
                Value = SchemeYearInt.ToString(),
                Active = true
            };

            string shortYear = SchemeYearInt.ToString().Substring(2);

            List<SchemeNameDrop> schemeNames = new List<SchemeNameDrop>()
            {
                new SchemeNameDrop
                {
                    Active = true,
                    Text = "CII" + shortYear
                },
                new SchemeNameDrop
                {
                    Active = true,
                    Text = "CIIXC" + shortYear
                },
                new SchemeNameDrop
                {
                    Active = true,
                    Text = "CIIT" + shortYear
                }
            };

            foreach (var name in schemeNames)
            {
                db.SchemeNameDrop.Add(name);
            }

            db.Control.Add(newYear);
            db.SaveChanges();

            return RedirectToAction("SchemeYearAdmin", "Admin");
        }

        public JsonResult PeopleSearch(string searchstring)
        {
            var model = holdingService.PersonList(searchstring);

            var response = model.Select(x => new { label = x.Name, value = x.StaffNumber }).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        private Holding ValidateInspector(Holding holding)
        {
            var validInspectors = db.Inspector.Select(x => x.staffNumber.ToUpper()).ToList();

            if (!validInspectors.Exists(x => x == holding.AssignedUser))
            {
                ModelState.AddModelError("AssignedUser", "Invalid M number entered - User " + holding.AssignedUser + " is not included on Inspectorate List");

                holding.AssignedDate = null;
            }

            return holding;
        }

        private void SetViewData(Holding holding)
        {
            ViewBag.SchemeNameDropId = new SelectList(db.SchemeNameDrop.AsNoTracking().Where(x => x.Active == true).OrderBy(x => x.SchemeNameDropId), "SchemeNameDropId", "Text", holding.SchemeNameDropId);
        }


    }
}