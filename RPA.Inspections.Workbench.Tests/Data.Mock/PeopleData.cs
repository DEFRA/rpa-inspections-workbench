using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Data.Mock
{
    public static class PeopleData
    {
        public static List<Person> Data()
        {
            return new List<Person>
            {
                new Person
                {

                    Id = Guid.NewGuid(),
                    Name = "Toward, Fay",
                    StaffNumber = "M600500",
                    Email = "Fay.Toward@Test.com"
                },
                new Person
                {
                    Id = Guid.NewGuid(),
                    Name = "Gordon, [REDACTED_NAME]",
                    StaffNumber = "M600300",
                    Email = "Lee.Gordon@Test.com"
                },
                new Person
                {
                    Id = Guid.Parse("4BA355C8-920D-420D-8E80-3664A0816458"),
                    Name = "Dormand, Scott",
                    StaffNumber = "M600400",
                    Email = "Scott@Test.com"
                }
            };
        }
    }
}
