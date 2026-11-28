using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.SL;
using BreedEntity = RPA.Inspections.Workbench.Models.Breed;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class ViewBreedModel : PageModel
    {
        private readonly IWorkbenchContext db;
        private readonly IBreedService breedService;

        public ViewBreedModel(IWorkbenchContext db, IBreedService breedService)
        {
            this.db = db;
            this.breedService = breedService;
        }

        [BindProperty]
        public BreedEntity Breed { get; set; }

        public IActionResult OnGet(Guid breedId)
        {
            Breed = breedService.GetBreedById(breedId);

            return Breed == null ? NotFound() : Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.SetModified(Breed);

            db.SaveChanges();

            return RedirectToPage("BreedList");
        }
    }
}
