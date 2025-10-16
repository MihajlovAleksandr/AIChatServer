using AIChatServer.Utils.Interfaces;
using System.Text;

namespace AIChatServer.Utils.Implementations
{
    public class StringChanger : IStringChanger
    {
        public string Replace(string text, string search, string replace, int count)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search) || count <= 0)
                return text ?? string.Empty;

            var sb = new StringBuilder(text.Length);
            int currentIndex = 0;
            int replacements = 0;

            while (replacements < count)
            {
                int index = text.IndexOf(search, currentIndex, StringComparison.Ordinal);
                if (index == -1)
                    break;

                sb.Append(text, currentIndex, index - currentIndex);
                sb.Append(replace);

                currentIndex = index + search.Length;
                replacements++;
            }

            if (currentIndex < text.Length)
            {
                sb.Append(text, currentIndex, text.Length - currentIndex);
            }

            return sb.ToString();
        }
    }
}
