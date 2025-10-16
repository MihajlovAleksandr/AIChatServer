using System.Text;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Utils.Implementations
{
    public class HttpService(HttpClient httpClient) : IHttpService
    {
        private readonly HttpClient _httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<string?> PostAsync(string url, string jsonBody, Dictionary<string, string> headers)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            foreach (var header in headers)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ответ от API: {responseBody}");
                    return responseBody;
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                    Console.WriteLine($"Детали ошибки: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при выполнении запроса: {ex.Message}");
            }

            return null;
        }
    }
}
