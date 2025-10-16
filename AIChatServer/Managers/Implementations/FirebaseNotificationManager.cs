using AIChatServer.Managers.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class FirebaseNotificationManager : INotificationManager
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly ILogger<FirebaseNotificationManager> _logger;

        public FirebaseNotificationManager(string path, ILogger<FirebaseNotificationManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                var credential = GoogleCredential.FromFile(path);
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential
                });

                _logger.LogInformation("FirebaseApp initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize FirebaseApp from path: {Path}", path);
                throw;
            }
        }

        private async Task SendMessageToDeviceInternal(string deviceToken, Dictionary<string, string> data)
        {
            if (_firebaseApp == null)
            {
                _logger.LogError("FirebaseApp is not initialized.");
                throw new InvalidOperationException("FirebaseApp is not initialized. Call InitializeFirebase() first.");
            }

            var message = new Message()
            {
                Token = deviceToken,
                Data = data,
                Android = new AndroidConfig()
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig()
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "apns-priority", "10" }
                    },
                    Aps = new Aps()
                    {
                        ContentAvailable = true
                    }
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Successfully sent data message to device {DeviceToken}: {Response}", deviceToken, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data message to device {DeviceToken}.", deviceToken);
            }
        }

        public async Task SendMessageToDevice(string deviceToken, string title, string body, Guid chatId, bool isBodyPrompt)
        {
            var data = new Dictionary<string, string>
            {
                { "title", title },
                { "body", body },
                { "chatId", chatId.ToString() },
                { "isBodyPrompt", isBodyPrompt.ToString() }
            };

            await SendMessageToDeviceInternal(deviceToken, data);
        }
    }
}
