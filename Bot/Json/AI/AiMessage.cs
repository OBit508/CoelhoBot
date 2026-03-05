using System;
using System.Collections.Generic;
using System.Text;

namespace CoelhoBot.Bot.Json.AI
{
    public class AiMessage
    {
        public string? content { get; set; } = "";
        public string? role { get; set; } = "system";
    }
}
