using CoelhoBot.Bot;
using CoelhoBot.Bot.Memory;
using System.Reflection;

BotManager.Assembly = Assembly.GetExecutingAssembly();
ConfigManager.Initialize();
MemoryManager.Initialize();
await BotManager.Initialize(args);