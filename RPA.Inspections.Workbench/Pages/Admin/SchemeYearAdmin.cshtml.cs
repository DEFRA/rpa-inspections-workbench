using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using ControlEntity = RPA.Inspections.Workbench.Models.Control;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class SchemeYearAdminModel : PageModel
    {
        private readonly IWorkbenchContext db;

        public SchemeYearAdminModel(IWorkbenchContext db)
        {
            this.db = db;
        }

        [BindProperty]
        public List<ControlEntity> Controls { get; set; }

        public void OnGet()
        {
            Controls = db.Control.AsNoTracking().Where(x => x.Property == "SchemeYr").OrderByDescending(p => p.Value).ToList();
        }

        public IActionResult OnPost()
        {
            foreach (var ctrl in Controls ?? new List<ControlEntity>())
            {
                string year = ctrl.Value.Substring(ctrl.Value.Length - 2);

                List<SchemeNameDrop> schemeNames = db.SchemeNameDrop.Where(x => x.Text.Substring(x.Text.Length - 2) == year).ToList();

                foreach (var name in schemeNames)
                {
                    name.Active = ctrl.Active;
                    db.SetModified(name);
                }

                db.SetModified(ctrl);
            }

            db.SaveChanges();

            return RedirectToPage();
        }

        // State-changing action: previously a GET (AddSchemeYear), now POST with anti-forgery
        public IActionResult OnPostAddNext()
        {
            string lastSchemeYear = db.Control.Where(x => x.Property == "SchemeYr").OrderByDescending(p => p.Value).Select(c => c.Value).FirstOrDefault();

            int schemeYearInt = int.Parse(lastSchemeYear) + 1;

            ControlEntity newYear = new ControlEntity
            {
                Property = "SchemeYr",
                Value = schemeYearInt.ToString(),
                Active = true
            };

            string shortYear = schemeYearInt.ToString().Substring(2);

            foreach (var text in new[] { "CII" + shortYear, "CIIXC" + shortYear, "CIIT" + shortYear })
            {
                db.SchemeNameDrop.Add(new SchemeNameDrop { Active = true, Text = text });
            }

            db.Control.Add(newYear);
            db.SaveChanges();

            return RedirectToPage();
        }
    }
}
