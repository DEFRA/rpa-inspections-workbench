using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.SL;
using X.PagedList;
using HoldingEntity = RPA.Inspections.Workbench.Models.Holding;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class HoldingListModel : PageModel
    {
        private readonly IHoldingService holdingService;

        public HoldingListModel(IHoldingService holdingService)
        {
            this.holdingService = holdingService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IPagedList<HoldingEntity> Holdings { get; private set; }

        public void OnGet()
        {
            Holdings = holdingService.GetAllHolding(SearchString, PageQuery.Page(Request), PageQuery.PageSize(Request, 50));
        }
    }
}
