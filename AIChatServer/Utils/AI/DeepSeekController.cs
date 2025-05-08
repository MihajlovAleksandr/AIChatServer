using AIChatServer.Entities.AI;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AIChatServer.Utils.AI
{
    public class DeepSeekController : IAIController
    {
        private readonly string apiKey;
        private readonly string apiUrl;
        private readonly int maxTokens;
        public DeepSeekController(int maxTokens)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var aiSettings = configuration.GetSection("AISettings");
            apiKey = aiSettings["apiKeyDeepSeek"] ?? throw new ArgumentNullException("apiKey");
            apiUrl = aiSettings["apiUrlDeepSeek"] ?? throw new ArgumentNullException("apiUrl");
            this.maxTokens = maxTokens;
        }
        public AIResponseInfo? Parse(string? aiResponse)
        {
            if (aiResponse == null) return null;
            try
            {
                using JsonDocument document = JsonDocument.Parse(aiResponse);
                JsonElement root = document.RootElement;
                string answer = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                int totalTokens = root.GetProperty("usage").GetProperty("total_tokens").GetInt32();

                return new AIResponseInfo
                {
                    Answer = answer,
                    TotalTokensUsed = totalTokens
                };
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid API response format", nameof(aiResponse), ex);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to parse API response", nameof(aiResponse), ex);
            }
        }

        public async Task<AIResponseInfo?> SendMessageAsync(List<AIMessage> aiResponse)
        {
            return Parse(await SendMessageToAPIAsync(aiResponse));
        }

        private async Task<string?> SendMessageToAPIAsync(List<AIMessage> aIMessages)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var requestBody = new
                {
                    model = "deepseek-chat",
                    messages = aIMessages,
                    stream = false,
                    max_tokens = maxTokens
                };
                string jsonRequestBody = JsonHelper.Serialize(requestBody);
                Console.WriteLine($"Sending to AI:\n{jsonRequestBody}\n\n");
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Ответ от API: {responseBody}");
                        return responseBody;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Детали ошибки: {errorBody}");
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
}
