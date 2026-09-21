using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.SL
{
    public interface IMailService
    {
        MailMessage sendPackRequest(string cph, Guid? HoldingId);
        MailMessage GenerateInspectorEmail(Holding model, PackRequested prmodel);
    }
}
