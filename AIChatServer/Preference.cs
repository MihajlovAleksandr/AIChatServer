using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class Preference
    {
        public int Id;
        public int MinAge;
        public int MaxAge;
        public char? Gender;
        public override string ToString()
        {
            return $"Preference {{{Id}}}:\n{Gender}{MinAge}-{MaxAge}";
        }
    }
}
