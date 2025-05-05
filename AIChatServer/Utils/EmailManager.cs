using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace AIChatServer.Utils
{
    public static class EmailManager
    {
        private static string smtpServer;
        private static int port;
        private static string senderEmail;
        private static string password;

        static EmailManager()
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

        public static void Send(string recipientEmail, string subject, string text, string imagePath)
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
                mailMessage.To.Add(recipientEmail);
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

        public static void SendVerificationCode(string recipientEmail, int verificationCode)
        {
            string code = $"{verificationCode / 1000} {verificationCode % 1000}";
            string text = File.ReadAllText("VerificationCodeEmail.html").Replace("[VERIFICATION_CODE]", code);
            Send(recipientEmail, $"{code} - Ваш верыфікацыйны код для уліковага запісу AIChat", text, "logo.png");
        }
    }
}
