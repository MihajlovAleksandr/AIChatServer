using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class PointsTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public DateTime TransactionTime { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return $"Transaction {{{Id}}}\n{Amount}->User{{{UserId}}} in {TransactionTime}\n{Description}";
        }
    }
}
