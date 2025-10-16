using AIChatServer.Utils.Interfaces;
using System.Net.Mail;

namespace AIChatServer.Utils.Implementations
{
    public class HtmlContentBuilder(IStringChanger stringChanger) : IHtmlContentBuilder
    {
        public AlternateView CreateHtmlAlternateView(string text, string[] imagePaths)
        {
            var linkedResources = GetLinkedResources(imagePaths);
            string htmlBody = PrepareText(text, linkedResources);

            var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
            foreach (var resource in linkedResources)
                view.LinkedResources.Add(resource);

            return view;
        }

        private string PrepareText(string text, LinkedResource[] images)
        {
            for (int i = 0; i < images.Length; i++)
            {
                text = stringChanger.Replace(text, "[IMAGE]", images[i].ContentId, 1);
            }
            return text;
        }

        private LinkedResource[] GetLinkedResources(string[] imagesPaths)
        {
            var linkedResources = new LinkedResource[imagesPaths.Length];
            for (int i = 0; i < imagesPaths.Length; i++)
            {
                linkedResources[i] = new LinkedResource(imagesPaths[i], "image/jpeg");
                linkedResources[i].ContentId = Guid.NewGuid().ToString();
            }
            return linkedResources;
        }
    }
}
