using AIChatServer.Integrations.Email.DTO;
using AIChatServer.Integrations.Email.Interfaces;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Utils.Implementations
{
    public class VerificationCodeSender (IEmailSender emailSender, IEmailTextGetter emailTextGetter) : IVerificationCodeSender
    {
        public void Send(string recipientEmail, int verificationCode, string localization)
        {
            string code = $"{verificationCode / 1000} {verificationCode % 1000}";
            EmailMessageRequest emailMessage = emailTextGetter.GetEmail(localization, "VerificationCode");
            emailMessage.Subject = emailMessage.Subject.Replace("[VERIFICATION_CODE]", code);
            emailMessage.Text = emailMessage.Text.Replace("[VERIFICATION_CODE]", code);
            emailSender.Send(recipientEmail, emailMessage, ["logo.png"]);
        }
    }
}
