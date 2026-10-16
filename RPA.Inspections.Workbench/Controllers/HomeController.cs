using Microsoft.AspNetCore.Authorization;
using RPA.Inspections.Workbench.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace RPA.Inspections.Workbench.Controllers
{
    [Authorize(Roles = "CI Inspections Workbench: Basic Access")]

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //Get Main Menu
            
               return View();

        }

    }
}