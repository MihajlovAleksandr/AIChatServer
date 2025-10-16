using AIChatServer.Utils.Interfaces;
using System.Net.Mail;
using System.Net;
using AIChatServer.Integrations.Email.Interfaces;
using AIChatServer.Integrations.Email.DTO;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Integrations.Email.Implementations
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _senderEmail;
        private readonly string _password;
        private readonly IHtmlContentBuilder _htmlContentBuilder;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(string smtpServer, int port, string senderEmail, string password,
                           IHtmlContentBuilder htmlContentBuilder, ILogger<EmailSender> logger)
        {
            _smtpServer = smtpServer ?? throw new ArgumentNullException(nameof(smtpServer));
            _port = port;
            _senderEmail = senderEmail ?? throw new ArgumentNullException(nameof(senderEmail));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _htmlContentBuilder = htmlContentBuilder ?? throw new ArgumentNullException(nameof(htmlContentBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Send(string email, EmailMessageRequest emailMessage, string[] imagePaths)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Recipient email is empty or null.");
                return;
            }

            if (emailMessage == null)
            {
                _logger.LogWarning("EmailMessageRequest is null.");
                return;
            }

            try
            {
                using var smtpClient = new SmtpClient(_smtpServer)
                {
                    Port = _port,
                    Credentials = new NetworkCredential(_senderEmail, _password),
                    EnableSsl = true,
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail),
                    Subject = emailMessage.Subject,
                    Body = emailMessage.Text,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                mailMessage.AlternateViews.Add(_htmlContentBuilder.CreateHtmlAlternateView(emailMessage.Text, imagePaths));

                smtpClient.Send(mailMessage);

                _logger.LogInformation("Email sent successfully to {RecipientEmail} with subject '{Subject}'.", email, emailMessage.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {RecipientEmail} with subject '{Subject}'.", email, emailMessage.Subject);
            }
        }
    }
}
