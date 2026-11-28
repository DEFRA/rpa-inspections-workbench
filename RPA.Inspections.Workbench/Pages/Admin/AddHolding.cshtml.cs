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
    public class AddHoldingModel : PageModel
    {
        private readonly IWorkbenchContext db;
        public AddHoldingModel(IWorkbenchContext db)
        {
            this.db = db;

        }

        [BindProperty]
        public HoldingEntity Holding { get; set; }

        public SelectList SchemeNames { get; private set; }

        public void OnGet()
        {
            Holding = new HoldingEntity();

            SetViewData();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid && SetSchemeName())
            {
                db.Holding.Add(Holding);

                db.SaveChanges();

                return RedirectToPage("HoldingList");
            }

            SetViewData();

            return Page();
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
