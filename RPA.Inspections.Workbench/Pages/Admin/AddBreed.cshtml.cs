using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.DAL;
using BreedEntity = RPA.Inspections.Workbench.Models.Breed;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class AddBreedModel : PageModel
    {
        private readonly IWorkbenchContext db;

        public AddBreedModel(IWorkbenchContext db)
        {
            this.db = db;
        }

        [BindProperty]
        public BreedEntity Breed { get; set; } = new BreedEntity();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Breed.Add(Breed);

            db.SaveChanges();

            return RedirectToPage("BreedList");
        }
    }
}
