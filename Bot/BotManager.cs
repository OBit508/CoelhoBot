using CoelhoBot.Bot.Json;
using CoelhoBot.Bot.Json.AI;
using CoelhoBot.Bot.Memory;
using CoelhoBot.Modules;
using CoelhoBot.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace CoelhoBot.Bot
{
    public static class BotManager
    {
        public static Assembly? Assembly;
        public static GatewayClient? Client;
        public static Settings? BotSettings;
        public static ConcurrentQueue<Task> Logging = new ConcurrentQueue<Task>();
        public static async Task Initialize(string[] args)
        {
            if (Assembly == null) return;
            BotSettings = null;
            using (Stream? stream = Assembly.GetManifestResourceStream("CoelhoBot.Assets.Settings.json"))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        BotSettings = JsonSerializer.Deserialize<Settings>(new StreamReader(stream).ReadToEnd());
                    }
                }
            }
            if (BotSettings != null)
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
                builder.Services.AddDiscordGateway(options =>
                {
                    options.Token = BotSettings.Token;
                    options.Intents = GatewayIntents.All;
                })
                .AddGatewayHandlers(Assembly)
                .AddApplicationCommands()
                .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>();
                IHost host = builder.Build();
                host.Services.GetRequiredService<ApplicationCommandService<ApplicationCommandContext>>().AddModules(Assembly);
                host.Services.GetRequiredService<ComponentInteractionService<StringMenuInteractionContext>>().AddModules(Assembly);
                Client = host.Services.GetRequiredService<GatewayClient>();
                Client.MessageCreate += OnMessageCreate;
                Client.Ready += OnReady;
                MemoryTaskHelper.ResetLoop();
                await host.RunAsync();
            }
        }
        public static async ValueTask OnReady(ReadyEventArgs readyEventArgs)
        {
            if (Client != null)
            {
                await Client.Rest.ModifyCurrentGuildUserAsync(1347318199586259036, delegate (CurrentGuildUserOptions guildUserOptions)
                {
                    guildUserOptions.Nickname = MemoryManager.Data?.LastName ?? "Coelho";
                });
            }
            Console.SetOut(new ConsoleWriter(delegate (string str)
            {
                if (Client != null)
                {
                    Logging.Enqueue(Client.Rest.SendMessageAsync(1456085997455544474, new MessageProperties() { Content = str }));
                }
            }));
            _ = LoopLogger();
        }
        public static async ValueTask OnMessageCreate(Message message)
        {
            if (message.Author.IsBot || message.ChannelId != 1456075446125858908) return; 
            MemoryManager.Waiting.Add((message.ChannelId, message.Id), new AiMessage()
            {
                role = "user",
                content = $"[User: {message.Author.Username}, ID: {message.Author.Id}]: {Helpers.ModifyMention(message)}"
            });
        }
        public static async Task LoopLogger()
        {
            while (true)
            {
                Task? task = null;
                if (Logging.TryDequeue(out task) && task != null && Client != null)
                {
                    await task;
                }
                await Task.Delay(200);
            }
        }
    }
}
