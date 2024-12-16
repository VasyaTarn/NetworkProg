using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProg
{
    public class ChatMessage
    {
        public string Login { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTime Moment { get; set; }

        public override string ToString()
        {
            return $"{Login}: {Text}";
        }

        public string GetSendTime()
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - Moment;

            if (elapsed.Days > 0)
            {
                return $"{elapsed.Days} д. назад";
            }
            else if (elapsed.TotalHours >= 1)
            {
                return $"{(int)elapsed.TotalHours} ч. назад";
            }
            else if (elapsed.TotalMinutes >= 1)
            {
                return $"{(int)elapsed.TotalMinutes} мин. назад";
            }
            else
            {
                return $"{(int)elapsed.TotalSeconds} сек. назад";
            }
        }
    }
}
