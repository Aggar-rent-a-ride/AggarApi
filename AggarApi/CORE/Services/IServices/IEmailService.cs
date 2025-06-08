using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlBody);
        Task<bool> SendEmailWithImageAsync(string recipientEmail, string subject, string htmlBody, byte[] image);
    }
}
