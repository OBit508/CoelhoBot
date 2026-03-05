using CoelhoBot.Bot.Json;
using CoelhoBot.Bot.Json.AI;
using CoelhoBot.Utilities;
using Microsoft.AspNetCore.Identity;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoelhoBot.Bot.Memory
{
    public static class MemoryManager
    {
        public static string MemoryFolder = Path.Combine(ConfigManager.ConfigDirectory, "Memories-v1");
        public static string DataPath = Path.Combine(ConfigManager.ConfigDirectory, "Data.json");
        public static Dictionary<(ulong channelId, ulong messageId), AiMessage> Waiting = new Dictionary<(ulong channelId, ulong messageId), AiMessage>();
        public static MemorySlot? CurrentMemory;
        public static BotData? Data;
        public static void Initialize()
        {
            void CreateData()
            {
                try
                {
                    File.WriteAllText(DataPath, JsonSerializer.Serialize(Data));
                    Logger.LogInfo("Data.json criado.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Houve um erro ao tentar criar o arquivo Data.json, mensagem de erro: {ex.Message}");
                }
            }
            if (!Directory.Exists(MemoryFolder))
            {
                Directory.CreateDirectory(MemoryFolder);
            }
            if (File.Exists(DataPath))
            {
                try
                {
                    Data = JsonSerializer.Deserialize<BotData>(File.ReadAllText(DataPath));
                    Logger.LogInfo("Data carregada.");
                }
                catch (Exception ex)
                {
                    Data = new BotData();
                    Logger.LogError($"Houve um erro ao tentar ler o arquivo Data.json, mensagem de erro: {ex.Message}");
                    File.Delete(DataPath);
                    Logger.LogInfo("Data.json deletado.");
                    CreateData();
                }
            }
            else
            {
                Data = new BotData();
                CreateData();
            }
            LoadMemory();
        }
        public static void LoadMemory()
        {
            if (Data == null) return;
            if (string.IsNullOrEmpty(Data.LastMemory))
            {
                Data.LastMemory = "Default";
                UpdateData();
            }
            string memoryPath = Path.Combine(MemoryFolder, Data.LastMemory);
            if (File.Exists(memoryPath))
            {
                try
                {
                    CurrentMemory = JsonSerializer.Deserialize<MemorySlot>(File.ReadAllText(memoryPath));
                    Logger.LogInfo("Memória carregada.");
                }
                catch (Exception ex)
                {
                    CurrentMemory = new MemorySlot() { Messages = new List<AiMessage>() };
                    Logger.LogError($"Houve um erro ao tentar ler o arquivo {memoryPath}, mensagem de erro: {ex.Message}");
                }
            }
            else
            {
                CurrentMemory = new MemorySlot() { Messages = new List<AiMessage>() };
                try
                {
                    File.WriteAllText(memoryPath, JsonSerializer.Serialize(CurrentMemory));
                    Logger.LogInfo($"{memoryPath} criado.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Houve um erro ao tentar criar o arquivo {memoryPath}, mensagem de erro: {ex.Message}");
                }
            }
        }
        public static async Task LoopReply(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (Waiting.Count > 0)
                    {
                        KeyValuePair<(ulong ChannelId, ulong MessageId), AiMessage> message = Waiting.Last();
                        CurrentMemory?.Messages?.AddRange(Waiting.Values);
                        Waiting.Clear();
                        AiResponse response = await Helpers.AskAsync(CreateAiMessageArray(), token);
                        if (response != null && response.content != null)
                        {
                            response.content = Helpers.ModifyAiText(response.content);
                            CurrentMemory?.Messages?.Add(new AiMessage() { role = "assistant", content = response.content });
                            if (CurrentMemory != null && CurrentMemory.Messages != null && ConfigManager.Config != null && ConfigManager.Config.MemoryLimit != null)
                            {
                                int count = CurrentMemory.Messages.Count - ConfigManager.Config.MemoryLimit.Value;
                                if (count > 0)
                                {
                                    for (int i = 0; i < count; i++)
                                    {
                                        CurrentMemory.Messages.RemoveAt(0);
                                    }
                                }
                            }
                            if (BotManager.Client != null && BotManager.Client.Rest != null)
                            {
                                try
                                {
                                    await BotManager.Client.Rest.SendMessageAsync(message.Key.ChannelId, new MessageProperties()
                                    {
                                        Content = response.content,
                                        MessageReference = MessageReferenceProperties.Reply(message.Key.MessageId)
                                    });
                                }
                                catch
                                {
                                    await BotManager.Client.Rest.SendMessageAsync(message.Key.ChannelId, new MessageProperties()
                                    {
                                        Content = response.content,
                                    });
                                }
                            }
                            await AiHelper.HandleCommand(response);
                        }
                        SaveMemory();
                    }
                }
                catch { }
                await Task.Delay(333, token);
            }
        }
        public static void UpdateData()
        {
            if (Data != null)
            {
                File.WriteAllText(DataPath, JsonSerializer.Serialize(Data));
            }
        }
        public static void SaveMemory()
        {
            if (CurrentMemory != null && Data != null)
            {
                if (string.IsNullOrEmpty(Data.LastMemory))
                {
                    Data.LastMemory = "Default";
                    UpdateData();
                }
                File.WriteAllText(Path.Combine(MemoryFolder, Data.LastMemory), JsonSerializer.Serialize(CurrentMemory));
            }
        }
        public static List<AiMessage> CreateAiMessageArray()
        {
            List<AiMessage> messages = Helpers.DefaultMessages.ToList();
            if (Data != null)
            {
                messages.AddRange(AiHelper.BuildInformations(Data));
            }
            if (CurrentMemory != null && CurrentMemory.Messages != null)
            {
                messages.AddRange(CurrentMemory.Messages);
            }
            return messages;
        }
    }
}