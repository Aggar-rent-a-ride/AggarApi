using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs.IBackgroundJobs
{
    public interface IEmailSendingJob
    {
        void SendEmail(string recipientEmail, string subject, string htmlBody);
    }
}
