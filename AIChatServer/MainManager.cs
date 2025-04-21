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
                    knownUser.SendCommand(UserManager.GetSyncDBCommand(knownUser.User.Id, DateTime.MinValue));
                    break;
                case "LoadUsersInChat":
                    int chatId = command.GetData<int>("chatId");
                    knownUser.SendCommand(GetLoadUsersInChatCommand(chatId));
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
