using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class User
    {
        public int id;
        public string? name;
        public string username;
        public DateTime? premium;
        public int age;
        public char? gender;
        public Preference Preference;
        public override string ToString()
        {
            return $"User {{{id}}}:\n{username}\n{gender}: {name} ({age}) \n{premium}";
        }
    }
}
