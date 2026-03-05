using CoelhoBot.Bot.Memory;
using CoelhoBot.Utilities;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoelhoBot.Modules
{
    public class SelectMenuInteraction : ComponentInteractionModule<StringMenuInteractionContext>
    {
        [ComponentInteraction(CustomIds.LoadMemory)]
        public async Task LoadMemory()
        {
            string mem = Context.SelectedValues[0];
            if (mem == MemoryManager.Data?.LastMemory)
            {
                await RespondAsync(InteractionCallback.ModifyMessage(delegate (MessageOptions messageOptions)
                {
                    messageOptions.Components = new IMessageComponentProperties[] { };
                    messageOptions.Embeds = new EmbedProperties[]
                    {
                        new EmbedProperties()
                        {
                            Title = "Erro",
                            Description = $"Você não pode carregar a memória que está em uso",
                            Color = BotColors.Coelho
                        }
                    };
                }));
                return;
            }
            MemoryTaskHelper.StopLoop();
            MemoryManager.SaveMemory();
            MemoryManager.Data?.LastMemory = mem;
            MemoryManager.UpdateData();
            MemoryManager.LoadMemory();
            MemoryTaskHelper.ResetLoop();
            await RespondAsync(InteractionCallback.ModifyMessage(delegate (MessageOptions messageOptions)
            {
                messageOptions.Components = new IMessageComponentProperties[] { };
                messageOptions.Embeds = new EmbedProperties[]
                {
                    new EmbedProperties() 
                    {
                        Title = "Memória carregada",
                        Description = $"A memória {mem} foi carregada.",
                        Color = BotColors.Coelho
                    }
                };
            }));
        }
        [ComponentInteraction(CustomIds.DeleteMemory)]
        public async Task DeleteMemory()
        {
            string mem = Context.SelectedValues[0];
            if (mem == MemoryManager.Data?.LastMemory)
            {
                await RespondAsync(InteractionCallback.ModifyMessage(delegate (MessageOptions messageOptions)
                {
                    messageOptions.Components = new IMessageComponentProperties[] { };
                    messageOptions.Embeds = new EmbedProperties[]
                    {
                        new EmbedProperties()
                        {
                            Title = "Erro",
                            Description = $"Você não pode apagar a memória que está em uso",
                            Color = BotColors.Coelho
                        }
                    };
                }));
                return;
            }
            string path = Path.Combine(MemoryManager.MemoryFolder, mem);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            await RespondAsync(InteractionCallback.ModifyMessage(delegate (MessageOptions messageOptions)
            {
                messageOptions.Components = new IMessageComponentProperties[] { };
                messageOptions.Embeds = new EmbedProperties[]
                {
                    new EmbedProperties()
                    {
                        Title = "Memória apagada",
                        Description = $"A memória {mem} foi apagada.",
                        Color = BotColors.Coelho
                    }
                };
            }));
        }
    }
}
