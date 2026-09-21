using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class ControlData
    {
        public static List<Models.Control> Data()
        {
            return new List<Models.Control>
            {
                new Models.Control
                {
                       Active = true,
                       ControlId = Guid.NewGuid(),
                       Property = "SchemeYr",
                       Value = "2019"
                },
                new Models.Control
                {
                       Active = true,
                       ControlId = Guid.NewGuid(),
                       Property = "SchemeYr",
                       Value = "2020"
                },
                new Models.Control
                {
                       Active = false,
                       ControlId = Guid.NewGuid(),
                       Property = "SchemeYr",
                       Value = "2017"
                }
            };
        }
    }
}
