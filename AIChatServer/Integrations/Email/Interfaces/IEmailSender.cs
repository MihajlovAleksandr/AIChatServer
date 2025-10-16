using AIChatServer.Integrations.Email.DTO;

namespace AIChatServer.Integrations.Email.Interfaces
{
    public interface IEmailSender
    {
        void Send(string email, EmailMessageRequest emailMessage, string[] imagePaths);
    }
}
