using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;

namespace RPA.Inspections.Workbench.Pages.BcmsAdmin
{
    public class IndexModel : PageModel
    {
        private readonly IWorkbenchContext db;

        public IndexModel(IWorkbenchContext db)
        {
            this.db = db;
        }

        public List<BcmsRequested> Requests { get; private set; }

        public void OnGet()
        {
            Requests = db.BcmsRequested.AsNoTracking().OrderByDescending(x => x.DatePackRequested).Take(10).ToList();
        }

        public IActionResult OnPostBulk(DateTime startDate, DateTime endDate)
        {
            var bcmsRequested = new BcmsRequested
            {
                BcmsPackRequestedId = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
                ActiveRequest = true,
                DatePackRequested = DateTime.Now,
                BulkRequest = true,
                SchemeYear = null
            };

            db.BcmsRequested.Add(bcmsRequested);
            db.SaveChanges();

            return RedirectToPage();
        }

        public IActionResult OnPostSpecific(string cph, int schemeYear)
        {
            var bcmsRequested = new BcmsRequested
            {
                BcmsPackRequestedId = Guid.NewGuid(),
                StartDate = null,
                EndDate = null,
                CphNumber = cph,
                ActiveRequest = true,
                DatePackRequested = DateTime.Now,
                SchemeYear = schemeYear,
                Filepath = null
            };

            db.BcmsRequested.Add(bcmsRequested);
            db.SaveChanges();

            return RedirectToPage();
        }
    }
}
