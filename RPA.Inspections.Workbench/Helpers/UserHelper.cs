using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Factory;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RPA.Inspections.Workbench.Helpers
{
    public class UserHelper : IUserHelper
    {

        IPeopleContext pdb;

        public UserHelper(IPeopleContext peopleContext)
        {
            this.pdb = peopleContext;
        }

        public string CurrentUser()

        {
            string user;

            var context = HttpContextManager.Current;

            if (context != null)
            {
                user = context.User.Identity.Name.ToLower()
                   .Replace("earth\\mo", "earth\\m")
                   .Replace("demeter\\mo", "demeter\\m");
                   
                user = Regex.Replace(user, @".*\\", "");
            }
            else
            {
                user = "Unknown";
            }

            return user;
        }

        public IEnumerable<string> GetAllStaffNumbersForCurrentUser()
        {
            var currentUser = CurrentUser();
            var userEmail = CurrentUserEmail(currentUser);
            IEnumerable<string> userNames = new List<string>();

            if (userEmail != null)
            {
                userNames = pdb.People
                    .Where(x => x.Email == userEmail)
                    .Select(x => x.StaffNumber)
                    .ToArray();
            }
           
            return userNames;
        }


        public string CurrentUserEmail(string user)
        {
            string usermail;

            var context = HttpContextManager.Current;

            if (context != null)
            {

                usermail = pdb.People.Where(x => x.StaffNumber == user).Select(x => x.Email).FirstOrDefault();

                if (usermail == "rpainternaldevelopmenttest@rpa.gsi.gov.uk")
                {
                    usermail = null;
                }
            }
            else
            {
                usermail = "Unknown";
            }

            return usermail;
        }

        public string CurrentUserName(string user)
        {
            string username;

            var context = HttpContextManager.Current;

            if (context != null)
            {

                username = pdb.People.Where(x => x.StaffNumber == user).Select(x => x.Name).FirstOrDefault();

            }
            else
            {
                username = "Unknown";
            }

            return username;
        }
    }
}