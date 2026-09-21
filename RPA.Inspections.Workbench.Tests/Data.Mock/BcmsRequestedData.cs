using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class BcmsRequestedData
    {

        public static List<BcmsRequested> Data()
        {
            return new List<BcmsRequested>
            {
                new BcmsRequested{BcmsPackRequestedId = Guid.Parse("EBC5578D-10C4-4D81-8555-00223D13A921"), StartDate = DateTime.Now, EndDate = DateTime.Now, CphNumber="01/001/1001", ActiveRequest =false, DatePackRequested=DateTime.Now, BulkRequest=false, BcmsGenerated=DateTime.Now, SchemeYear=2020, Filepath="test1" },
                new BcmsRequested{BcmsPackRequestedId = Guid.Parse("FDA2F72D-31AF-4B70-9C7E-C19C1CFA9D84"), StartDate = DateTime.Now, EndDate = DateTime.Now, CphNumber="01/001/1002", ActiveRequest = false, DatePackRequested=DateTime.Now, BulkRequest=false, BcmsGenerated=DateTime.Now, SchemeYear=2020, Filepath="test2" },
                new BcmsRequested{BcmsPackRequestedId = Guid.Parse("0B84C3A0-FA43-4234-A418-F9B580967D6A"), StartDate = DateTime.Now, EndDate = DateTime.Now, CphNumber="01/001/1003", ActiveRequest = true, DatePackRequested=DateTime.Now, BulkRequest=true, BcmsGenerated=DateTime.Now, SchemeYear=2020, Filepath="test3" }
            };
        }
    }
}
