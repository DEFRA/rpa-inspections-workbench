using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IFileService fileService;
        private readonly IAuthorizationService authorization;

        public IndexModel(IFileService fileService, IAuthorizationService authorization)
        {
            this.fileService = fileService;
            this.authorization = authorization;
        }

        public void OnGet()
        {
        }

        // POST: Add a new inspection list (SuperUser only, on top of the Admin policy applied to the folder)
        public async Task<IActionResult> OnPostAsync(IFormFile bulkSource, bool deleteAll)
        {
            if (!(await authorization.AuthorizeAsync(User, ApplicationServices.SuperUserPolicy)).Succeeded)
            {
                return Forbid();
            }

            try
            {
                fileService.RunAddList(bulkSource, deleteAll);

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToPage("FileLoadFail");
            }
        }
    }
}
