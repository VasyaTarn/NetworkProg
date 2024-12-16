using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProg
{
    public class ClientRequest
    {
        public string Command { get; set; } = null!;
        public ChatMessage ChatMessage { get; set; } = null!;
    }
}
