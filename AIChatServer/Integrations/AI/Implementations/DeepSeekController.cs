using AIChatServer.Integrations.AI.DTO;
using AIChatServer.Integrations.AI.Interfaces;
using AIChatServer.Utils.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIChatServer.Integrations.AI.Implementations
{
    public class DeepSeekController : IAIController
    {
        private readonly int _maxTokens;
        private readonly ISerializer _serializer;
        private readonly IHttpService _httpService;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ILogger<DeepSeekController> _logger;

        public DeepSeekController(
            int maxTokens,
            ISerializer serializer,
            IHttpService httpService,
            string apiKey,
            string apiUrl,
            ILogger<DeepSeekController> logger)
        {
            _maxTokens = maxTokens;
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static AIMessageResponse? Parse(string? aiResponse, ILogger logger)
        {
            if (aiResponse == null)
            {
                logger.LogWarning("AI response is null.");
                return null;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(aiResponse);
                JsonElement root = document.RootElement;

                string? answer = root
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                int totalTokens = root
                    .GetProperty("usage")
                    .GetProperty("total_tokens")
                    .GetInt32();

                logger.LogInformation("AI response parsed successfully. Tokens used: {TotalTokens}", totalTokens);

                return new AIMessageResponse
                {
                    Answer = answer,
                    TotalTokensUsed = totalTokens
                };
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to parse AI response: invalid JSON format.");
                throw new ArgumentException("Invalid API response format", nameof(aiResponse), ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse AI response.");
                throw new ArgumentException("Failed to parse API response", nameof(aiResponse), ex);
            }
        }

        public async Task<AIMessageResponse?> SendMessageAsync(List<AIMessageRequest> aiMessages)
        {
            if (aiMessages == null || aiMessages.Count == 0)
            {
                _logger.LogWarning("No AI messages provided to send.");
                return null;
            }

            var requestBody = new
            {
                model = "deepseek-chat",
                messages = aiMessages,
                stream = false,
                max_tokens = _maxTokens
            };

            string jsonRequestBody = _serializer.Serialize(requestBody);

            _logger.LogInformation("Sending AI request:\n{RequestBody}", jsonRequestBody);

            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {_apiKey}" },
                { "Accept", "application/json" }
            };

            try
            {
                string? response = await _httpService.PostAsync(_apiUrl, jsonRequestBody, headers);
                _logger.LogInformation("Received AI response: {Response}", response);

                return Parse(response, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to AI endpoint.");
                throw;
            }
        }
    }
}
