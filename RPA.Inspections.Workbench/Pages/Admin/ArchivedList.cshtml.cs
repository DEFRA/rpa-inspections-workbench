using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.SL;
using HoldingEntity = RPA.Inspections.Workbench.Models.Holding;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class ArchivedListModel : PageModel
    {
        private readonly IWorkbenchContext db;
        private readonly IHoldingService holdingService;

        public ArchivedListModel(IWorkbenchContext db, IHoldingService holdingService)
        {
            this.db = db;
            this.holdingService = holdingService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchYear { get; set; }

        public List<string> ArchivedYears { get; private set; }

        public List<HoldingEntity> Holdings { get; private set; }

        public void OnGet()
        {
            ArchivedYears = db.Control.Where(x => x.Property == "SchemeYr" && x.Active == false).OrderBy(c => c.Value).Select(y => y.Value).ToList();

            Holdings = holdingService.GetAllArchivedHoldings(SearchString, ArchivedYears, SearchYear);
        }
    }
}
