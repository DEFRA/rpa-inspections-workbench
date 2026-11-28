using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.SL;
using X.PagedList;
using BreedEntity = RPA.Inspections.Workbench.Models.Breed;

namespace RPA.Inspections.Workbench.Pages.Admin
{
    public class BreedListModel : PageModel
    {
        private readonly IBreedService breedService;

        public BreedListModel(IBreedService breedService)
        {
            this.breedService = breedService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public IPagedList<BreedEntity> Breeds { get; private set; }

        public void OnGet()
        {
            Breeds = breedService.GetBreeds(SearchString, PageQuery.Page(Request), PageQuery.PageSize(Request, 50));
        }
    }
}
