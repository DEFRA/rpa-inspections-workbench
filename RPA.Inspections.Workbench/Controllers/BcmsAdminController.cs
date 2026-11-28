using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Services;

namespace RPA.Inspections.Workbench.Controllers
{
    // The BCMS admin screen is a Razor Page (Pages/BcmsAdmin); this controller keeps the report download endpoint.
    [Authorize(Policy = ApplicationServices.BcmsPolicy)]
    public class BcmsAdminController : Controller
    {
        private readonly IWorkbenchContext db;
        private readonly IFileStorage fileStorage;

        public BcmsAdminController(IWorkbenchContext db, IFileStorage fileStorage)
        {
            this.db = db;
            this.fileStorage = fileStorage;
        }

        public IActionResult open_report(string name)
        {
            // Only reports that were generated for a BCMS request can be opened
            if (!db.BcmsRequested.Any(x => x.Filepath == name))
            {
                return NotFound();
            }

            try
            {
                return File(fileStorage.Download(name), "text/html");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
