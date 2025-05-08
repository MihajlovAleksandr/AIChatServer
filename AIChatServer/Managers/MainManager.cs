using AIChatServer.Entities.Chats;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User;
using AIChatServer.Entities.User.ServerUsers;
using AIChatServer.Utils;
using AIChatServer.Utils.AI;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace AIChatServer.Managers
{
    public class MainManager
    {
        private ChatManager chatManager;
        private UserManager userManager;
        private AIManager aIManager;
        public MainManager()
        {
            int aiId = 1;
            chatManager = new ChatManager(aiId, 100);
            userManager = new UserManager();
            aIManager = new AIManager(aiId, new DeepSeekController(70), DB.GetAIMessagesByChat(chatManager.GetUserChats(aiId)));
            userManager.CommandGot += OnCommandGot;
            chatManager.OnChatCreated += OnChatCreated;
            chatManager.OnChatEnded += OnChatEnded;
            userManager.OnConnectionEvent += OnConnectionEvents;
            userManager.IsChatSearching += chatManager.IsSearchingChat;
            aIManager.OnSendMessage += OnAISendMessage;
        }
        private void OnConnectionEvents(object? sender, bool isOnline)
        {
            ServerUser serverUser = (ServerUser)sender;
            Command command = new Command("UserOnlineChanges");
            command.AddData("userId", serverUser.User.Id);
            command.AddData("isOnline", isOnline);
            userManager.SendCommand(DB.GetUsersInSameChats(serverUser.User.Id), command);
        }
        

        private async void OnCommandGot(object? sender, Command command)
        {
            KnownUser knownUser = (KnownUser)sender;
            switch (command.Operation)
            {
                case "SendMessage":
                    Message message = command.GetData<Message>("message");
                    if (message.Sender != knownUser.User.Id)
                    {
                        ServerUser.SendCommand(command.Sender, new Command("LogOut"));
                        return;
                    }
                    await SendMessageAsync(message);
                    break;
                case "SearchChat":
                    string type = command.GetData<string>("param");
                    Command seachChatCommand = new Command("SearchChat");
                    seachChatCommand.AddData("isChatSearching", true);
                    knownUser.SendCommand(seachChatCommand);
                    await chatManager.SearchChatAsync(knownUser.User, type);
                    break;
                case "EndChat":
                    chatManager.EndChat(command.GetData<int>("chatId"));
                    break;
                case "StopSearchingChat":
                    chatManager.StopSearchingChat(knownUser.User.Id);
                    Command stopSeachChatCommand = new Command("SearchChat");
                    stopSeachChatCommand.AddData("isChatSearching", false);
                    knownUser.SendCommand(stopSeachChatCommand);
                    break;
                case "SyncDB":
                    int syncDBUserId = knownUser.User.Id;
                    ServerUser.SendCommand(command.Sender , userManager.GetSyncDBCommand(syncDBUserId, DateTime.MinValue));
                    break;
                case "LoadUsersInChat":
                    int chatId = command.GetData<int>("chatId");
                    ServerUser.SendCommand(command.Sender,GetLoadUsersInChatCommand(chatId));
                    break;
                case "GetSettingsInfo":
                    Command getSettingsInfoCommand = new Command("GetSettingsInfo");
                    getSettingsInfoCommand.AddData("email", knownUser.User.Email);
                    getSettingsInfoCommand.AddData("userData", knownUser.User.UserData);
                    getSettingsInfoCommand.AddData("preference", knownUser.User.Preference);
                    getSettingsInfoCommand.AddData("devices", DB.GetConnectionCount(knownUser.User.Id));
                    getSettingsInfoCommand.AddData("emailNotifications", DB.GetNotifications(knownUser.User.Id));
                    ServerUser.SendCommand(command.Sender, getSettingsInfoCommand);
                    break;
                case "UpdateUserData":
                    UserData userData = command.GetData<UserData>("userData");
                    if (DB.UpdateUserData(userData, knownUser.User.Id))
                    {
                        knownUser.User.UserData = userData;
                        Command userDataCommand = new Command("UserDataUpdated");
                        userDataCommand.AddData("userData", userData);
                        knownUser.SendCommand(userDataCommand);
                    }
                    break;
                case "UpdatePreference":
                    Preference preference = command.GetData<Preference>("preference");
                    if (DB.UpdatePreference(preference, knownUser.User.Id))
                    {
                        knownUser.User.Preference = preference;
                        Command preferenceCommand = new Command("PreferenceUpdated");
                        preferenceCommand.AddData("preference", preference);
                        knownUser.SendCommand(preferenceCommand);
                    }
                    break;
                case "DeleteConnection":
                    int connectinId = command.GetData<int>("connectionId");
                    if (connectinId == default) connectinId = command.Sender.Id;
                    ConnectionInfo connectionInfo = DB.RemoveConnection(connectinId);
                    if (connectionInfo!=null)
                    {
                        knownUser.SendCommand(connectinId, new Command("Logout"));
                        Connection? connection = knownUser.RemoveConnection(connectinId);
                        if (connection is not null)
                            userManager.CreateUnknownUser(connection);
                        else
                            DB.DeleteUnknownConnection(connectinId);
                        Command deleteCommand = new Command("DeleteConnection");
                        deleteCommand.AddData("connectionInfo", connectionInfo);
                        deleteCommand.AddData("count", DB.GetConnectionCount(connectionInfo.UserId));
                        knownUser.SendCommand(deleteCommand);

                    }
                    break;
                case "ChangePassword":
                    if(DB.VerifyPassword(command.GetData<string>("currentPassword"), knownUser.User.Password))
                    {
                        string newPassword = DB.ChangePassword(knownUser.User.Id, command.GetData<string>("newPassword"));
                        if (!string.IsNullOrEmpty(newPassword))
                        {
                            ServerUser.SendCommand(command.Sender, new Command("PasswordChanged"));
                            knownUser.User.Password = newPassword;
                        }
                    }
                    break;
                case "GetDevices":
                    var devicesList = DB.GetAllUserConnections(knownUser.User.Id);
                    Command devicesCommand = new Command("GetDevices");
                    devicesCommand.AddData("devices", devicesList);
                    devicesCommand.AddData("currentConnectionId", command.Sender.Id);
                    ServerUser.SendCommand(command.Sender, devicesCommand);
                    break;
                case "EntryTokenRead":
                    string token = command.GetData<string>("token");
                    if (TokenManager.ValidateEntryToken(token, out int userId))
                    {
                        if(!userManager.KnowUser(userId, knownUser.User))
                        {
                            knownUser.SendCommand(new Command("LoginInViaQRFailed"));
                        }
                    }
                    break;
                case "UpdateNotifications":
                    DB.UpdateNotifications(knownUser.User.Id, command.GetData<bool>("notifications"));
                    break;
                case "EmailNotifications":
                    bool emailNotificationsEnabled = command.GetData<bool>("enabled");
                    DB.UpdateNotifications(knownUser.User.Id, emailNotificationsEnabled);
                    Command emailNotificationsCommand = new Command("EmailNotifications");
                    emailNotificationsCommand.AddData("enabled", emailNotificationsEnabled);
                    knownUser.SendCommand(emailNotificationsCommand);
                    break;
                case "MainActivityState":
                    DB.SetLastOnline(command.Sender.Id, command.GetData<bool>("isOnline"));
                    break;
            }
        }
        private void OnChatCreated(Chat chat)
        {
            if (chat.ContainsAI(aIManager.AIId))
            {
                aIManager.CreateDialog(chat.Id);
            }
            Command command = new Command("CreateChat");
            command.AddData("chat", chat);
            userManager.SendCommand(chat.Users, command);
        }
        private void OnChatEnded(Chat chat)
        {
            if (chat.ContainsAI(aIManager.AIId))
            {
                aIManager.EndDialog(chat.Id);
            }
            Command command = new Command("EndChat");
            command.AddData("chat", chat);
            userManager.SendCommand(chat.Users, command);
        }
        private Command GetLoadUsersInChatCommand(int chatId)
        {
            Command command = new Command("LoadUsersInChat");
            var data = DB.LoadUsers(chatId);
            int aiIndex = data.Item1.IndexOf(aIManager.AIId);
            string chatType = chatManager.GetChatType(chatId);
            if (aiIndex != -1)
            {
                data.Item3[aiIndex] = true;
            }
            if (chatType == "random")
            {
                data.Item2[aiIndex] = new UserData() { Age = 0, Name = "Random", Gender = '-' };
                data.Item3[aiIndex] = true;
            }
            command.AddData("ids", data.Item1);
            command.AddData("userData", data.Item2);
            command.AddData("isOnline", data.Item3);
            return command;
        }
        private async Task SendMessageAsync(Message message)
        {
            message = DB.SendMessage(message);
            int[] users = chatManager.GetUsersInChat(message.Chat);
            Command sendMessageCommand = new Command("SendMessage");
            sendMessageCommand.AddData("message", message);
            userManager.SendCommand(users, sendMessageCommand);
            if (users.Contains(aIManager.AIId))
            {
                await aIManager.SendMessageAsync(message.Chat, message.Text);
            }
        }
        private void OnAISendMessage(Message message)
        {
            message = DB.SendMessage(message);
            int[] allUsers = chatManager.GetUsersInChat(message.Chat);
            int[] users = allUsers.Where(user => user != aIManager.AIId).ToArray();
            Command sendMessageCommand = new Command("SendMessage");
            sendMessageCommand.AddData("message", message);
            userManager.SendCommand(users, sendMessageCommand);
        }

    }
}
