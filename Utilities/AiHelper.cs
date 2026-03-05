using CoelhoBot.Bot;
using CoelhoBot.Bot.Json;
using CoelhoBot.Bot.Json.AI;
using CoelhoBot.Bot.Memory;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoelhoBot.Utilities
{
    public static class AiHelper
    {
        public static async Task HandleCommand(AiResponse response)
        {
            if (response.commands != null)
            {
                try
                {
                    foreach (string command in response.commands)
                    {
                        if (command.StartsWith("changename "))
                        {
                            string str = command.Replace("changename ", "");
                            if (!string.IsNullOrEmpty(str) && BotManager.Client != null)
                            {
                                await BotManager.Client.Rest.ModifyCurrentGuildUserAsync(1347318199586259036, delegate (CurrentGuildUserOptions guildUserOptions)
                                {
                                    guildUserOptions.Nickname = str;
                                });
                                Logger.LogInfo($"Coelho mudou o próprio nome para {str}");
                                MemoryManager.Data?.LastName = str;
                                MemoryManager.UpdateData();
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.LogError(ex.Message); }
            }
        }
        public static AiMessage[] BuildInformations(BotData botData)
        {
            return new AiMessage[]
            {
                new AiMessage()
                {
                    role = "system",
                    content = "Informações recentes:\n" +
                    $"Nome atual: {botData.LastName ?? "Coelho"};\n" +
                    $"SourceCode: https://github.com/OBit508/CoelhoBot"
                }
            };
        }
    }
}
