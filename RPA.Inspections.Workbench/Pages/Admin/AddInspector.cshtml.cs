using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.DAL;
using InspectorEntity = RPA.Inspections.Workbench.Models.Inspector;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class AddInspectorModel : PageModel
    {
        private readonly IWorkbenchContext db;

        public AddInspectorModel(IWorkbenchContext db)
        {
            this.db = db;
        }

        [BindProperty]
        public InspectorEntity Inspector { get; set; } = new InspectorEntity();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!db.Inspector.Any(x => x.staffNumber == Inspector.staffNumber))
            {
                db.Inspector.Add(Inspector);

                db.SaveChanges();
            }

            return RedirectToPage("InspectorList");
        }
    }
}
