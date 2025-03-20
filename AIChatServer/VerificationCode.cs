using Org.BouncyCastle.Crypto.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class VerificationCode
    {
        private readonly int code;
        private readonly DateTime validTo;
        public int Code { get { return code; } }
        public bool isVerify { get; private set; } 
        public VerificationCode()
        {
            Random random = new();
            //code = random.Next(100000, 999999);
            code = 111111;
            validTo = DateTime.Now.AddMinutes(15);
            isVerify = false;
        }
        public int Validate(int code)
        {
            if (this.code == code && !isVerify)
            {
                if (validTo > DateTime.Now)
                {
                    isVerify = true;
                    return 1;
                }
                return -1;
            }
            return 0;
        }
    }
}
