using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer.Entities.AI
{
    public class AIResponseInfo
    {
        public string Answer { get; set; }
        public int TotalTokensUsed { get; set; }
    }
}
