using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    public class CommandModule : BaseCommandModule
    {
        [Command("раб")]
        public async Task GreetCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("Мы не будем рабами.");
        }

        [Command("скажи")]
        public async Task SayCommand(CommandContext ctx, [RemainingText] string text)
        {
            var msg = await new DiscordMessageBuilder()
                .WithContent(text)
                .HasTTS(true)
                .SendAsync(ctx.Channel);
        }

    }
}
