using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer.Managers
{
    public class NotificationManager
    {
        private FirebaseApp _firebaseApp;

        public NotificationManager()
        {
            var credential = GoogleCredential.FromFile("aichatFirebase.json");

            _firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = credential
            });

        }

        public async Task SendMessageToDevice(string deviceToken, Dictionary<string, string> data)
        {
            if (_firebaseApp == null)
            {
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
                Console.WriteLine($"Successfully sent data message: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
        public async Task SendMessageToDevice(string deviceToken, string title, string body, int chatId, bool isBodyPrompt)
        {
            await SendMessageToDevice(deviceToken, new Dictionary<string, string>
            {
                { "title", title },
                { "body", body },
                { "chatId", chatId.ToString() },
                { "isBodyPrompt", isBodyPrompt.ToString() }
            });
        }
    }
}
