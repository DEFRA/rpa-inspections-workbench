using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;

namespace RPA.Inspections.Workbench.Pages.Holding
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PacksRequestedModel : PageModel
    {
        private readonly IWorkbenchContext db;
        private readonly IPackRequested packRequestedService;

        public PacksRequestedModel(IWorkbenchContext db, IPackRequested packRequestedService)
        {
            this.db = db;
            this.packRequestedService = packRequestedService;
        }

        public List<PackRequested> Packs { get; private set; }

        public void OnGet()
        {
            Packs = packRequestedService.GetOutstandingPackRequests();
        }

        // State-changing action: previously a GET (DeletePack), now POST with anti-forgery
        public IActionResult OnPostDeletePack(string savePath, Guid packID)
        {
            // Only delete the file recorded against this pack request
            if (!db.PackRequested.Any(x => x.packRequestedId == packID && x.savePath == savePath))
            {
                return NotFound();
            }

            packRequestedService.DeletePack(savePath, packID);

            return RedirectToPage();
        }
    }
}
