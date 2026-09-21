using System.Net.Mail;

namespace RPA.Inspections.Workbench.Services
{
    public interface ISendService
    {
        void sendEmail(MailMessage mail);
    }
}