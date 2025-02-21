using System;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AIChatServer
{
    public class EmailManager
    {
        private string smtpServer;
        private int port;
        private string senderEmail;
        private string password;

        public EmailManager()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            // Получение настроек Email
            var settings = configuration.GetSection("EmailSettings");
            smtpServer = settings["SmtpServer"];
            port = Convert.ToInt32(settings["SmtpPort"]);
            senderEmail = settings["SenderEmail"];
            password = settings["Password"];
        }

        public void SendWithImage(string recipientEmail, string subject, string text, string imagePath)
        {
            try
            {
                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = port,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = text,
                    IsBodyHtml = true,
                };

                // Добавление получателя
                mailMessage.To.Add(recipientEmail);

                // Загрузка картинки и добавление её в письмо
                var inlineImage = new LinkedResource(imagePath, "image/jpeg");
                inlineImage.ContentId = Guid.NewGuid().ToString();
                var htmlBody = text.Replace("[IMAGE]", $"<img src='cid:{inlineImage.ContentId}'/>");

                var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                view.LinkedResources.Add(inlineImage);
                mailMessage.AlternateViews.Add(view);

                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        public void SendWithoutImage(string recipientEmail, string subject, string text)
        {
            try
            {
                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = port,
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = text,
                    IsBodyHtml = true,
                };

                // Добавление получателя
                mailMessage.To.Add(recipientEmail);

                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
