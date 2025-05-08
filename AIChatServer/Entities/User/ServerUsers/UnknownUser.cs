using AIChatServer.Entities.Connection;
using AIChatServer.Utils;

namespace AIChatServer.Entities.User.ServerUsers
{
    public class UnknownUser : ServerUser
    {
        public event Action<UnknownUser> UserChanged;
        private VerificationCode verificationCode;
        public int Id { get; private set; }
        public UnknownUser(Connection.Connection connection, int id) : base(connection)
        {
            User = new User();
            User.Id = id;
            Id = id;
            base.GotCommand += (s, e) => GotCommand(e);
            Disconnected += (s, e) => { DB.DeleteUnknownConnection(connection.Id); };
        }
        private void GotCommand(Command command) {

            Console.WriteLine(command);
            switch (command.Operation)
            {
                case "GetEntryToken":
                    Command entryTokenCommand =new Command("EntryToken");
                    entryTokenCommand.AddData("token", TokenManager.GenerateEntryToken(User.Id));
                    SendCommand(command.Sender, entryTokenCommand);
                    break;
                case "LoginIn":
                    string email = command.GetData<string>("email");
                    string password = command.GetData<string>("password");
                    User = DB.LoginIn(email, password);
                    if (User != null)
                    {
                        KnowUser();
                    }
                    else
                    {
                        SendCommand(command.Sender, new Command("LoginInFailed"));
                    }
                        break;
                case "Registration":
                    email = command.GetData<string>("email");
                    if (DB.IsEmailFree(email))
                    {
                        password = command.GetData<string>("password");
                        User = new User(email, password);
                        verificationCode = new VerificationCode();
                        Console.WriteLine(verificationCode.Code);
                        EmailManager.SendVerificationCode(User.Email, verificationCode.Code);
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
                    User.UserData = data;
                    SendCommand(command.Sender, new Command("UserDataAdded"));
                    break;
                case "AddPreference":
                    Preference preference = command.GetData<Preference>("preference");
                    if (preference != null)
                    {
                        User.Preference = preference;
                    }
                    User.Id = (int)DB.AddUser(User, command.Sender.Id);
                    
                    KnowUser();
                    break;
            }
        }
        private void KnowUser()
        {
            UserChanged.Invoke(this);
        }
        public void SetUser(User user)
        {
            User = user;
        }
    }
}
