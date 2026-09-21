using System.Collections.Generic;

namespace RPA.Inspections.Workbench.Helpers
{
    public interface IUserHelper
    {
        string CurrentUser();
        string CurrentUserEmail(string user);
        string CurrentUserName(string user);
        IEnumerable<string> GetAllStaffNumbersForCurrentUser();
    }
}