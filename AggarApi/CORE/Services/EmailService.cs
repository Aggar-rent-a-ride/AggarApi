using CORE.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CORE.Helpers;
using CORE.DTOs.Auth;
using Microsoft.Extensions.Options;
using CORE.DTOs.Email;
using Microsoft.Extensions.Logging;

namespace CORE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlBody)
        {
            try
            {
                _logger.LogInformation("Attempting to send email to {RecipientEmail} with subject '{Subject}'", recipientEmail, subject);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.Value.EmailAddress, _emailSettings.Value.DisplayName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true // Set to true if sending an HTML email
                };

                mailMessage.To.Add(recipientEmail);

                // Configure the SMTP client
                using (var smtpClient = new SmtpClient(_emailSettings.Value.SmtpHost, _emailSettings.Value.SmtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_emailSettings.Value.EmailAddress,
                    _emailSettings.Value.Password);
                    smtpClient.EnableSsl = true; 

                    await smtpClient.SendMailAsync(mailMessage);
                }

                _logger.LogInformation("Email successfully sent to {RecipientEmail}", recipientEmail);
                return true; // email sent successfully
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {RecipientEmail} with subject '{Subject}'", recipientEmail, subject);
                return false; // email failed to send
            }
        }
    }
}
