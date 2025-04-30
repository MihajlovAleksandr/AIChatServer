using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class MainManager
    {
        ChatManager chatManager;
        UserManager userManager;
        public MainManager()
        {
            chatManager = new ChatManager();
            userManager = new UserManager();
            userManager.CommandGot += OnCommandGot;
            chatManager.OnChatCreated += OnChatCreated;
            chatManager.OnChatEnded += OnChatEnded;
            userManager.OnConnectionEvent += OnConnectionEvents;
        }
        private void OnConnectionEvents(object? sender, bool isOnline)
        {
            ServerUser serverUser = (ServerUser)sender;
            Command command = new Command("UserOnlineChanges");
            command.AddData("userId", serverUser.User.Id);
            command.AddData("isOnline", isOnline);
            userManager.SendCommand(DB.GetUsersInSameChats(serverUser.User.Id), command);
        }
        
        

        private void OnCommandGot(object? sender, Command command)
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
                    message = DB.SendMessage(message);
                    Command sendMessageCommand = new Command("SendMessage");
                    sendMessageCommand.AddData("message", message);
                    userManager.SendCommand(chatManager.GetUsersInChat(message.Chat), sendMessageCommand);
                    break;
                case "SearchChat":
                    chatManager.SearchChat(knownUser.User);
                    break;
                case "EndChat":
                    chatManager.EndChat(command.GetData<int>("chatId"));
                    break;
                case "StopSearchingChat":
                    chatManager.StopSearchingChat(knownUser.User.Id);
                    break;
                case "SyncDB":
                    ServerUser.SendCommand(command.Sender ,UserManager.GetSyncDBCommand(knownUser.User.Id, DateTime.MinValue));
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
                        if (!String.IsNullOrEmpty(newPassword))
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
            }
        }
        private void OnChatCreated(Chat chat)
        {
            Command command = new Command("CreateChat");
            command.AddData("chat", chat);
            userManager.SendCommand(chat.Users, command);
        }
        private void OnChatEnded(Chat chat)
        {
            Command command = new Command("EndChat");
            command.AddData("chat", chat);
            userManager.SendCommand(chat.Users, command);
        }
        private Command GetLoadUsersInChatCommand(int chatId)
        {
            Command command = new Command("LoadUsersInChat");
            var data = DB.LoadUsers(chatId);
            command.AddData("ids", data.Item1);
            command.AddData("userData", data.Item2);
            command.AddData("isOnline", data.Item3);
            return command;
        }

    }
}
