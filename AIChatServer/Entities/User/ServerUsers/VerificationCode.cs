namespace AIChatServer.Entities.User.ServerUsers
{
    public class VerificationCode
    {
        private readonly int _code;
        private readonly DateTime _validTo;
        private bool _isVerified;

        public int Code { get { return _code; } }

        public VerificationCode(Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            _code = random.Next(100000, 999999);
            _validTo = DateTime.Now.AddMinutes(15);
            _isVerified = false;
        }

        public int Validate(int code)
        {
            if (_code == code && !_isVerified)
            {
                if (_validTo > DateTime.Now)
                {
                    _isVerified = true;
                    return 1;
                }
                return -1;
            }
            return 0;
        }
    }
}
