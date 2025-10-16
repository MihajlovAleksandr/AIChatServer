namespace AIChatServer.Utils.Interfaces
{
    public interface IVerificationCodeSender
    {
        void Send(string recipientEmail, int verificationCode, string localization);
    }
}
