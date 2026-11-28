using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.SL;
using HoldingEntity = RPA.Inspections.Workbench.Models.Holding;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class ViewArchivedHoldingModel : PageModel
    {
        private readonly IHoldingService holdingService;

        public ViewArchivedHoldingModel(IHoldingService holdingService)
        {
            this.holdingService = holdingService;
        }

        public HoldingEntity Holding { get; private set; }

        public IActionResult OnGet(Guid holdingId)
        {
            Holding = holdingService.GetArchivedHoldingById(holdingId);

            return Holding == null ? NotFound() : Page();
        }
    }
}
