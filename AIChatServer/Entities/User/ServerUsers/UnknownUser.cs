using AIChatServer.Entities.Connection;
using AIChatServer.Utils;
using Google.Apis.Auth;

namespace AIChatServer.Entities.User.ServerUsers
{
    public class UnknownUser : ServerUser
    {
        public event Action<UnknownUser> UserChanged;
        private VerificationCode verificationCode;
        private readonly string googleClientId;
        public int Id { get; private set; }
        public UnknownUser(Connection.Connection connection, int id, string googleClientId) : base(connection)
        {
            User = new User();
            User.Id = id;
            Id = id;
            base.GotCommand += (s, e) => GotCommand(e);
            Disconnected += (s, e) => { DB.DeleteUnknownConnection(connection.Id); };
            this.googleClientId = googleClientId;
        }
        private async void GotCommand(Command command) {

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
                        if(User.GetRegistrationType() == RegistrationType.Google)
                        {
                            Command loginInFailed = new Command("UseOtherLoginInService");
                            loginInFailed.AddData("service", "Google");
                            SendCommand(loginInFailed);
                            return;
                        }
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
                        User = new User(email, password, RegistrationType.Password);
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
                case "SendGoogleTokenCommand":
                    string token = command.GetData<string>("token");
                    GoogleJsonWebSignature.Payload payload = await ValidateGoogleToken(token);

                    if (payload == null)
                    {
                        Console.WriteLine("payload == null");
                        break;
                    }
                    if(payload.Audience.ToString() != googleClientId)
                    {
                        Console.WriteLine($"payload.Audience.ToString() != clientId:\n{payload.Audience.ToString()} != {googleClientId}");
                        break;
                    }
                    User = DB.GetUserByEmail(payload.Email);
                    if (User == null)
                    {
                        User = new User(payload.Email, payload.Subject, RegistrationType.Google);
                        SendCommand(command.Sender, new Command("GoogleRegistrationSuccess"));
                        return;
                    }
                    else if (User.GetRegistrationType() == RegistrationType.Password)
                    {
                        Command loginInFailed = new Command("UseOtherLoginInService");
                        loginInFailed.AddData("service", "Password");
                        SendCommand(loginInFailed);
                        return;
                    }
                    if (!DB.VerifyGoogleId(payload.Email, payload.Subject))
                    {
                        return;
                    }
                    KnowUser();
                    Console.WriteLine($"Email: {payload.Email}\n Name: {payload.Name}");
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
        private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    ForceGoogleCertRefresh = true,
                    ExpirationTimeClockTolerance = TimeSpan.FromMinutes(5)
                };

                return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch
            {
                return null;
            }
        }
    }
}
