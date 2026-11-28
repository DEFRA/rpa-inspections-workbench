using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using HoldingEntity = RPA.Inspections.Workbench.Models.Holding;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class ViewHoldingModel : PageModel
    {
        private readonly IWorkbenchContext db;
        private readonly IHoldingService holdingService;

        public ViewHoldingModel(IWorkbenchContext db, IHoldingService holdingService)
        {
            this.db = db;
            this.holdingService = holdingService;
        }

        [BindProperty]
        public HoldingEntity Holding { get; set; }

        public SelectList SchemeNames { get; private set; }

        public IActionResult OnGet(Guid holdingId)
        {
            Holding = holdingService.GetHoldingById(holdingId);

            if (Holding == null)
            {
                return NotFound();
            }

            SetViewData();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Holding.AssignedUser != null)
            {
                Holding.AssignedUser = Holding.AssignedUser.ToUpper();

                ValidateInspector();
            }

            if (ModelState.IsValid && SetSchemeName())
            {
                db.SetModified(Holding);

                db.SaveChanges();

                return RedirectToPage("HoldingList");
            }

            SetViewData();

            return Page();
        }

        private void ValidateInspector()
        {
            var validInspectors = db.Inspector.Select(x => x.staffNumber.ToUpper()).ToList();

            if (!validInspectors.Exists(x => x == Holding.AssignedUser))
            {
                ModelState.AddModelError("Holding.AssignedUser", "Invalid M number entered - User " + Holding.AssignedUser + " is not included on Inspectorate List");

                Holding.AssignedDate = null;
            }
        }

        private bool SetSchemeName()
        {
            SchemeNameDrop schemeNameDrop = db.SchemeNameDrop.FirstOrDefault(x => x.SchemeNameDropId == Holding.SchemeNameDropId);

            if (schemeNameDrop == null)
            {
                ModelState.AddModelError("Holding.SchemeNameDropId", "Select a scheme name");

                return false;
            }

            Holding.schemeYear = "20" + schemeNameDrop.Text.Substring(schemeNameDrop.Text.Length - 2);

            Holding.SchemeName = schemeNameDrop.Text;

            return true;
        }

        private void SetViewData()
        {
            SchemeNames = new SelectList(db.SchemeNameDrop.AsNoTracking().Where(x => x.Active == true).OrderBy(x => x.SchemeNameDropId), "SchemeNameDropId", "Text", Holding.SchemeNameDropId);
        }
    }
}
