using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.SL;
using HoldingEntity = RPA.Inspections.Workbench.Models.Holding;

namespace RPA.Inspections.Workbench.Pages.Holding
{
    public class ViewHoldingModel : PageModel
    {
        private readonly IHoldingService holdingService;
        private readonly IUserHelper userHelper;

        public ViewHoldingModel(IHoldingService holdingService, IUserHelper userHelper)
        {
            this.holdingService = holdingService;
            this.userHelper = userHelper;
        }

        public HoldingEntity Holding { get; private set; }

        public string UserMail { get; private set; }

        public IActionResult OnGet(Guid holdingId)
        {
            Holding = holdingService.GetHoldingById(holdingId);

            if (Holding == null)
            {
                return NotFound();
            }

            UserMail = userHelper.CurrentUserEmail(userHelper.CurrentUser());

            return Page();
        }
    }
}
