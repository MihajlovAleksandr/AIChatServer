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
        

    }
}
