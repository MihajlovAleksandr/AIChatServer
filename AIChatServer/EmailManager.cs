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

        public void Send(string recipientEmail, string subject, string text, string imagePath)
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
                var htmlBody = text.Replace("[IMAGE]", inlineImage.ContentId);

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
        public void SendVerificationCode(string recipientEmail, int verificationCode)
        {
            string code = $"{verificationCode/1000} {verificationCode%1000}";
            string text = File.ReadAllText("VerificationCodeEmail.html").Replace("[VERIFICATION_CODE]",code);
            Send(recipientEmail, $"{code} - Ваш верыфікацыйны код для уліковага запісу AIChat", text, "logo.png");
        }
    }
}
