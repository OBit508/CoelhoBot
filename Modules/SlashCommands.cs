using CoelhoBot.Bot.Json;
using CoelhoBot.Bot.Json.AI;
using CoelhoBot.Bot.Memory;
using CoelhoBot.Utilities;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoelhoBot.Modules
{
    public class SlashCommands : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("create", "Crie uma nova memória")]
        public async Task Create([SlashCommandParameter(Name = "nome", Description = "Nome da memória")] string memory)
        {
            if (Context.Guild != null)
            {
                try
                {
                    if (!Context.Guild.Users[Context.User.Id].RoleIds.Contains(GuildRoles.AmigoDoCoelho))
                    {
                        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties() { Content = $"Num é <@&{GuildRoles.AmigoDoCoelho}> >:(" }));
                        return;
                    }
                }
                catch
                {
                    await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties() { Content = $"Num é <@&{GuildRoles.AmigoDoCoelho}> >:(" }));
                    return;
                }
            }
            if (Directory.GetFiles(MemoryManager.MemoryFolder).Any(f => Path.GetFileNameWithoutExtension(f) == memory))
            {
                await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
                {
                    Embeds = new EmbedProperties[]
                    {
                        new EmbedProperties()
                        {
                            Title = "Erro",
                            Description = $"Já existe uma memória com esse nome.\nNome: {memory}",
                            Color = new Color(194, 124, 14)
                        }
                    },
                }));
                return;
            }
            File.WriteAllText(Path.Combine(MemoryManager.MemoryFolder, memory), JsonSerializer.Serialize(new MemorySlot() { Messages = new List<AiMessage>() }));
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
            {
                Embeds = new EmbedProperties[]
                {
                    new EmbedProperties()
                    {
                        Title = "Memória criada",
                        Description = $"Foi criada uma memória nova.\nNome: {memory}",
                        Color = new Color(194, 124, 14)
                    }
                }
            }));
        }
        [SlashCommand("load", "Carregue uma memória salva")]
        public async Task Load()
        {
            if (Context.Guild != null)
            {
                try
                {
                    if (!Context.Guild.Users[Context.User.Id].RoleIds.Contains(GuildRoles.AmigoDoCoelho))
                    {
                        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties() { Content = $"Num é <@&{GuildRoles.AmigoDoCoelho}> >:(" }));
                        return;
                    }
                }
                catch
                {
                    await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties() { Content = $"Num é <@&{GuildRoles.AmigoDoCoelho}> >:(" }));
                    return;
                }
            }
            List<StringMenuSelectOptionProperties> stringMenuSelectOptionProperties = new List<StringMenuSelectOptionProperties>();
            foreach (string memory in Directory.GetFiles(MemoryManager.MemoryFolder))
            {
                string m = Path.GetFileNameWithoutExtension(memory);
                stringMenuSelectOptionProperties.Add(new StringMenuSelectOptionProperties(m, m));
            }
            StringMenuProperties menu = new StringMenuProperties(CustomIds.LoadMemory)
            {
                Placeholder = "Memória",
                Options = stringMenuSelectOptionProperties
            };
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Embeds = new EmbedProperties[]
                {
                    new EmbedProperties()
                    {
                        Title = "Escolha a memória",
                        Description = $"Selecione a memória salva que será carregada.\nAtual: {MemoryManager.Data?.LastMemory}",
                        Color = new Color(194, 124, 14)
                    }
                },
                Components = [menu],
                Flags = MessageFlags.Ephemeral,
            }));
        }
        [SlashCommand("delete", "Apague uma memória salva")]
        public async Task Delete()
        {
            if (Context.Guild != null)
            {
                try
                {
                    if (!Context.Guild.Users[Context.User.Id].RoleIds.Contains(GuildRoles.AmigoDoCoelho))
                    {
                        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties() { Content = $"Num é <@&{GuildRoles.AmigoDoCoelho}> >:(" }));
                        return;
                    }
                }
                catch
                {
                    await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties() { Content = $"Num é <@&{GuildRoles.AmigoDoCoelho}> >:(" }));
                    return;
                }
            }
            List<StringMenuSelectOptionProperties> stringMenuSelectOptionProperties = new List<StringMenuSelectOptionProperties>();
            foreach (string memory in Directory.GetFiles(MemoryManager.MemoryFolder))
            {
                string m = Path.GetFileNameWithoutExtension(memory);
                stringMenuSelectOptionProperties.Add(new StringMenuSelectOptionProperties(m, m));
            }
            StringMenuProperties menu = new StringMenuProperties(CustomIds.DeleteMemory)
            {
                Placeholder = "Memória",
                Options = stringMenuSelectOptionProperties
            };
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Embeds = new EmbedProperties[]
                {
                    new EmbedProperties()
                    {
                        Title = "Escolha a memória",
                        Description = $"Selecione a memória salva que será deletada.\nAtual: {MemoryManager.Data?.LastMemory}",
                        Color = new Color(194, 124, 14)
                    }
                },
                Components = [menu],
                Flags = MessageFlags.Ephemeral,
            }));
        }
    }
}
