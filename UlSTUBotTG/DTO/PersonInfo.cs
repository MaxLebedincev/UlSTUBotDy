using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UlSTUBotTG.DTO
{
    internal class PersonInfo
    {
        public long Id { get; set; }
        public Chat Chat { get; set; }
        public string NameGroup { get; set; } = "";
    }
}
