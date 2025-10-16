using AIChatServer.Integrations.Email.DTO;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Utils.Implementations
{
    public class EmailTextGetter : IEmailTextGetter
    {
        public EmailMessageRequest GetEmail(string localization, string subject)
        {
            return new EmailMessageRequest(
                File.ReadAllText($"emails\\{localization}\\{subject}\\subject.txt"),
                File.ReadAllText($"emails\\{localization}\\{subject}\\text.html"));
        }
    }
}
