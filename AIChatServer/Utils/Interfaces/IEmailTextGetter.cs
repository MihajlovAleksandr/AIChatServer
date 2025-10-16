using AIChatServer.Integrations.Email.DTO;

namespace AIChatServer.Utils.Interfaces
{
    public interface IEmailTextGetter
    {
        EmailMessageRequest GetEmail(string localization, string subject);
    }
}
