namespace AIChatServer.Config.Data
{
    public record EmailConfigData(
        string SmtpServer,
        int SmtpPort,
        string SenderEmail,
        string Password
    );
}
