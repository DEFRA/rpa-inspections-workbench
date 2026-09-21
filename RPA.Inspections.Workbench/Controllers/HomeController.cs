using RPA.Inspections.Workbench.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Controllers
{
    [Authorize(Roles = "CI Inspections Workbench: Basic Access")]

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Get Main Menu
            
               return View();

        }

    }
}