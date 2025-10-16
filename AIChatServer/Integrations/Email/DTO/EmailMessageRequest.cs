namespace AIChatServer.Integrations.Email.DTO
{
    public class EmailMessageRequest (string subject, string text)
    {
        public string Subject { get; set; } = subject;
        public string Text { get; set; } = text;
    }
}
