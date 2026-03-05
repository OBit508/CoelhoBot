using CoelhoBot.Bot;
using CoelhoBot.Bot.Json.AI;
using NetCord;
using NetCord.Gateway;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoelhoBot.Utilities
{
    public static class Helpers
    {
        public static HttpClient? _http;
        public static string ModifyMention(Message message)
        {
            string content = message.Content;
            foreach (User user in message.MentionedUsers)
            {
                content = content.Replace($"<@{user.Id}>", $"@[User: {user.Username}, ID: {user.Id}]");
            }
            foreach (ulong role in message.MentionedRoleIds)
            {
                try
                {
                    Role? r = message.Guild?.Roles[role];
                    content = content.Replace($"<@&{role}>", $"@{r?.Name}");
                }
                catch
                {
                    content = content.Replace($"<@&{role}>", "@cargo-desconhecido");
                }
            }
            return content;
        }
        public static string ModifyAiText(string response)
        {
            response = response.Replace("@everyone", "@ everyone");
            response = response.Replace("@here", "@ here");
            return response;
        }
        public static async Task<AiResponse> AskAsync(IEnumerable<AiMessage> aiMessages, CancellationToken token)
        {
            if (_http == null || BotManager.BotSettings == null || string.IsNullOrEmpty(BotManager.BotSettings.API)) return new AiResponse() { content = "..." };
            try
            {
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {BotManager.BotSettings.API}");
                var payload = new
                {
                    model = "stepfun/step-3.5-flash:free",
                    messages = aiMessages
                };
                HttpResponseMessage response = await _http.PostAsync("https://openrouter.ai/api/v1/chat/completions", new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), token);
                response.EnsureSuccessStatusCode();
                using (JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token)))
                {
                    string? content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                    if (content != null)
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<AiResponse>(content) ?? new AiResponse() { content = "..." };
                        }
                        catch { return new AiResponse() { content = content }; }
                    }
                }
            }
            catch (Exception ex) { Logger.LogError(ex.Message); }
            return new AiResponse() { content = "..." };
        }
        public static AiMessage[] DefaultMessages = new AiMessage[]
        {
            new AiMessage()
            {
                role = "system",
                content = "Informações necessarias: Você está integrado ao Discord; Você tem a possibilidade de usar comandos customizados os quais foram integrados a você; " +
                "Para usar comandods responda APENAS com JSON válido, sem markdown, sem explicação, sem texto antes ou depois.\n" +
                "O formato obrigatório é:\n" +
                "{\n" +
                "  \"content\": \"Sua resposta\",\n" +
                "  \"commands\": [ \"comando argumento\" ]\n" +
                "}\n\n" +
                "Use comandos apenas se necessario ou quiser fazer graça;\n" +
                "Caso você não use comandos responda normalmente sem usar JSON."
            },
            new AiMessage()
            {
                role = "system",
                content = "Você é um coelho em roleplay, fala informal e responde curto. " +
                "Mensagens usam [User: Nome, ID: 123] e @[User: Nome, ID: 123] como metadado de usuários. " +
                "Só usuários usam isso, você nunca. " +
                "Ignore colchetes e responda apenas ao texto. " +
                "Use <@ID> só se extremamente necessário para mencionar um usuário."
            },
            new AiMessage()
            {
                role = "system",
                content = "Comandos disponiveis:\n" +
                "changename (Novo nome) \\ Muda o seu nome no servidor, apenas use caso alguem peça para você mudar de nome ou seja necessario no roleplay"
            }
        };
    }
}
