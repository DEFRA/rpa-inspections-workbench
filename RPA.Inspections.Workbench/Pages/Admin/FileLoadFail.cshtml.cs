using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class FileLoadFailModel : PageModel
    {
        public string ErrorMessage { get; private set; }

        public void OnGet()
        {
            ErrorMessage = TempData["ErrorMessage"] as string;
        }
    }
}
