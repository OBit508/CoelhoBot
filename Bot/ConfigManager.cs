using CoelhoBot.Bot.Json;
using CoelhoBot.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CoelhoBot.Bot
{
    public static class ConfigManager
    {
        public static string ConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Coelho");
        public static BotConfig? Config = new BotConfig();
        public static void Initialize()
        {
            if (BotManager.Assembly == null) return;
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
            string configFile = Path.Combine(ConfigDirectory, "Config.json");
            if (File.Exists(configFile))
            {
                try
                {
                    Config = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText(configFile));
                    Logger.LogInfo("Configurações carregadas.");
                }
                catch (Exception ex)
                {
                    Config = LoadFromAssembly(BotManager.Assembly);
                    Logger.LogError($"Houve um erro ao tentar ler o arquivo Config.json, mensagem de erro: {ex.Message}");
                }
            }
            else
            {
                Config = LoadFromAssembly(BotManager.Assembly);
                try
                {
                    File.WriteAllText(configFile, JsonSerializer.Serialize(Config));
                    Logger.LogInfo("Config.json criado.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Houve um erro ao tentar criar o arquivo Config.json, mensagem de erro: {ex.Message}");
                }
            }
            Helpers._http = new HttpClient() { Timeout = TimeSpan.FromSeconds(Config?.HttpTimeout ?? 30) };
        }
        public static BotConfig? LoadFromAssembly(Assembly assembly)
        {
            BotConfig? botConfig = null;
            using (Stream? stream = assembly.GetManifestResourceStream("CoelhoBot.Assets.Config.json"))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        botConfig = JsonSerializer.Deserialize<BotConfig>(new StreamReader(stream).ReadToEnd());
                    }
                }
            }
            return botConfig;
        }
    }
}
