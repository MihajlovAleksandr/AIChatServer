using System.Net.WebSockets;

namespace AIChatServer
{
    public class UnknownUser : ServerUser
    {
        public event EventHandler<User> UserChanged;
        public User user { get; private set; }
        private VerificationCode verificationCode;
        public UnknownUser(Connection connection, int id) : base(connection)
        {
            user = new User();
            user.Id = id;
            base.GotCommand += (s, e) => GotCommand(e);
        }
        private void GotCommand(Command command) {

            Console.WriteLine(command);
            switch (command.Operation)
            {
                case "GetEntryToken":
                    Command entryTokenCommand =new Command("EntryToken");
                    entryTokenCommand.AddData("token", TokenManager.GenerateEntryToken(user.Id));
                    SendCommand(command.Sender, entryTokenCommand);
                    break;
                case "LoginIn":
                    string email = command.GetData<string>("email");
                    string password = command.GetData<string>("password");
                    KnowUser(command.Sender, DB.LoginIn(email, password));
                    break;
                case "Registration":
                    email = command.GetData<string>("email");
                    if (DB.IsEmailFree(email))
                    {
                        password = command.GetData<string>("password");
                        user = new User(email, password);
                        verificationCode = new VerificationCode();
                        Console.WriteLine(verificationCode.Code);
                        //EmailManager.SendVerificationCode(user.email, verificationCode.Code);
                        SendCommand(command.Sender, new Command("VerificationCodeSend"));
                    }
                    else
                    {
                        SendCommand(command.Sender, new Command("EmailIsBusy"));
                    }
                        break;
                case "VerificationCode":
                    int code = command.GetData<int>("code");
                    Command returnCommand = new Command("VerificationCodeAnswer");
                    returnCommand.AddData("answer", verificationCode.Validate(code));
                    SendCommand(command.Sender, returnCommand);
                    break;
                case "AddUserData":
                    UserData data = command.GetData<UserData>("userData");
                    user.UserData = data;
                    SendCommand(command.Sender, new Command("UserDataAdded"));
                    break;
                case "AddPreference":
                    Preference preference = command.GetData<Preference>("preference");
                    if (preference != null)
                    {
                        user.Preference = preference;
                    }
                    user.Id = (int)DB.AddUser(user);
                    KnowUser(command.Sender, user);
                    break;
            }
        }
        private void KnowUser(Connection connection,User user)
        {
            if (user != null)
            {
                UserChanged.Invoke(connection, user);
            }
        }
    }
}
