using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
      public class Message : IComparable<Message>
    {
        public int Id { get; set; }
        public int Chat { get; set; }
        public int User {  get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public Message() { User = -1; Time = DateTime.Now;}

        public int CompareTo(Message other)
        {
            return -1*Time.CompareTo(other.Time);
        }
    }
}
