using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProg
{
    public class ServerResponse
    {
        public string Status { get; set; } = null!;
        public IEnumerable<ChatMessage>? Messages { get; set; } = null!;
    }
}
