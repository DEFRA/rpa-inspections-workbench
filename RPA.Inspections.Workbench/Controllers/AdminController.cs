using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;

namespace RPA.Inspections.Workbench.Controllers
{
    // The Admin screens are Razor Pages (Pages/Admin); this controller keeps the JSON endpoint used by their autocomplete.
    [Authorize(Policy = ApplicationServices.AdminPolicy)]
    public class AdminController : Controller
    {
        private readonly IHoldingService holdingService;

        public AdminController(IHoldingService holdingService)
        {
            this.holdingService = holdingService;
        }

        public JsonResult PeopleSearch(string searchstring)
        {
            var model = holdingService.PersonList(searchstring);

            var response = model.Select(x => new { label = x.Name, value = x.StaffNumber }).ToList();

            return Json(response);
        }
    }
}
