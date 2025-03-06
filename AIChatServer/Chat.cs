using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class Chat : IComparable<Chat>
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int[] Users { get; set; }
        public Chat()
        {
            Time = DateTime.Now;
        }

        public int CompareTo(Chat other)
        {
            return Time.CompareTo(other.Time);
        }
    }
}
