using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.Services.IServices;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CORE.BackgroundJobs
{
    public class EmailSendingJob
    {
        private readonly ILogger<UserRatingUpdateJob> _logger;
        private readonly IEmailService _emailService;

        public EmailSendingJob(ILogger<UserRatingUpdateJob> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }
        public void SendEmail(string recipientEmail, string subject, string htmlBody)
        {
            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(recipientEmail, subject, htmlBody));
        }
    }
}
