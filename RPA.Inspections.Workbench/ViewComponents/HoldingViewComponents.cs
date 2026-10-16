using Microsoft.AspNetCore.Mvc;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;

namespace RPA.Inspections.Workbench.ViewComponents
{
    // Replace the child actions (Html.RenderAction) that used to render parts of the Holding screens.

    public class GeneratedPacksViewComponent : ViewComponent
    {
        private readonly IPackRequested packRequestedService;

        public GeneratedPacksViewComponent(IPackRequested packRequestedService)
        {
            this.packRequestedService = packRequestedService;
        }

        public IViewComponentResult Invoke()
        {
            var model = packRequestedService.GetDownloadableRequests();

            return View(model);
        }
    }

    public class ViewLinkedHoldingsViewComponent : ViewComponent
    {
        private readonly ILinkedHoldingService linkedHoldingService;

        public ViewLinkedHoldingsViewComponent(ILinkedHoldingService linkedHoldingService)
        {
            this.linkedHoldingService = linkedHoldingService;
        }

        public IViewComponentResult Invoke(Guid holdingId)
        {
            List<LinkedHolding> model = linkedHoldingService.GetLinkedholdingById(holdingId);

            ViewBag.linkedIdHolding = holdingId;

            return View(model);
        }
    }

    public class ViewCattleViewComponent : ViewComponent
    {
        private readonly ICattleService cattleService;

        public ViewCattleViewComponent(ICattleService cattleService)
        {
            this.cattleService = cattleService;
        }

        public IViewComponentResult Invoke(Guid holdingId)
        {
            List<Cattle> model = cattleService.GetCattleByHoldingId(holdingId);

            ViewBag.cattleIdholding = holdingId;

            return View(model);
        }
    }

    public class LateCattleViewComponent : ViewComponent
    {
        private readonly ICattleService cattleService;

        public LateCattleViewComponent(ICattleService cattleService)
        {
            this.cattleService = cattleService;
        }

        public IViewComponentResult Invoke(Guid holdingId)
        {
            List<Cattle> model = cattleService.GetLateCattle(holdingId);

            ViewBag.cattleIdholding = holdingId;

            return View(model);
        }
    }

    public class CountCattleViewComponent : ViewComponent
    {
        private readonly ICattleService cattleService;

        public CountCattleViewComponent(ICattleService cattleService)
        {
            this.cattleService = cattleService;
        }

        public IViewComponentResult Invoke(Guid holdingId)
        {
            List<Cattle> model = cattleService.GetCattleByHoldingId(holdingId);

            ViewBag.cattleIdholding = holdingId;

            ViewBag.List1 = model.Where(x => x.listNumber == "LIST1").Count();

            ViewBag.List2 = model.Where(x => x.listNumber == "LIST2").Count();

            ViewBag.Other = model.Where(x => x.listNumber == "").Count();

            return View(model);
        }
    }

    public class ViewPackRequestedViewComponent : ViewComponent
    {
        private readonly IPackRequested packRequestedService;

        public ViewPackRequestedViewComponent(IPackRequested packRequestedService)
        {
            this.packRequestedService = packRequestedService;
        }

        public IViewComponentResult Invoke(Guid holdingId)
        {
            PackRequested model = packRequestedService.GetPackRequestedById(holdingId);

            ViewBag.PackReqHoldingId = holdingId;

            return View(model);
        }
    }
}
