using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class UserData
    {
        public int Id { get; set; }
        public char Gender { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public bool IsFits(Preference preference)
        {
            if (preference.Gender[0] == Gender || preference.Gender=="Any")
            {
                if (Age > preference.MinAge && Age < preference.MaxAge)
                {
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            return $"UserData {{{Id}}}\n{Gender}{Age}\n{Name}";
        }
    }
}
