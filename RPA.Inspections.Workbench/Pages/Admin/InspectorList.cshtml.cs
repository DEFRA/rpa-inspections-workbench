using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class InspectorListModel : PageModel
    {
        private readonly IWorkbenchContext db;

        public InspectorListModel(IWorkbenchContext db)
        {
            this.db = db;
        }

        public List<Inspector> Inspectors { get; private set; }

        public void OnGet()
        {
            Inspectors = db.Inspector.AsNoTracking().ToList();
        }

        // State-changing action: previously a GET (RemoveInspector), now POST with anti-forgery
        public IActionResult OnPostRemove(Guid inspectorId)
        {
            Inspector inspector = db.Inspector.FirstOrDefault(x => x.inspectorId == inspectorId);

            if (inspector == null)
            {
                return NotFound();
            }

            db.Inspector.Remove(inspector);

            db.SaveChanges();

            return RedirectToPage();
        }
    }
}
