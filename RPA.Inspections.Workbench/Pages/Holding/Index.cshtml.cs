using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.SL;
using X.PagedList;
using HoldingEntity = RPA.Inspections.Workbench.Models.Holding;

namespace RPA.Inspections.Workbench.Pages.Holding
{
    public class IndexModel : PageModel
    {
        private readonly IHoldingService holdingService;
        private readonly IUserHelper userHelper;

        public IndexModel(IHoldingService holdingService, IUserHelper userHelper)
        {
            this.holdingService = holdingService;
            this.userHelper = userHelper;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IPagedList<HoldingEntity> Holdings { get; private set; }

        public string LoggedInUser { get; private set; }

        public void OnGet()
        {
            int page = PageQuery.Page(Request);
            int pageSize = PageQuery.PageSize(Request, 10);

            Holdings = holdingService.GetHolding(string.IsNullOrEmpty(SearchString) ? null : SearchString, page, pageSize);

            if (!string.IsNullOrEmpty(SearchString) && Holdings.Count == 0)
            {
                TempData["InvalidCPH"] = "Invalid CPH Entered or CPH not Found";
            }

            LoggedInUser = userHelper.CurrentUser();
        }
    }
}
