using System.Net.Mail;

namespace AIChatServer.Utils.Interfaces
{
    public interface IHtmlContentBuilder
    {
        AlternateView CreateHtmlAlternateView(string text, string[] imagePaths);
    }
}
