using Bot.Models;
using Bot.Modules;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Bot
{
    public class Program
    {
        public readonly EventId BotEventId = new EventId(42, "HomeBot");
        
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public VoiceNextExtension Voice { get; set; }
        public LavalinkExtension Lavalink { get; set; }

        public static void Main(string[] args)
        {
            var prog = new Program();
            prog.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            var json = "";
            using (var fs = File.OpenRead("conf.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();
            
            var cfgjson = JsonConvert.DeserializeObject<BotConfig>(json);
            var cfg = new DiscordConfiguration()
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true

            };

            this.Client = new DiscordClient(cfg);

            

            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;


            var commandCfg = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { cfgjson.CommandPrefix },

                EnableDms = true,

                EnableMentionPrefix = true
            };

            var endpoint = new ConnectionEndpoint
            {
                Hostname = cfgjson.IPLavalink,
                Port = cfgjson.PortLavalink
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = cfgjson.PasswordLavalink,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            this.Commands = this.Client.UseCommandsNext(commandCfg);

            this.Commands.RegisterCommands<CommandModule>();

            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;
            

            this.Voice = this.Client.UseVoiceNext();

            this.Lavalink = this.Client.UseLavalink();

            this.Commands.RegisterCommands<MusicCommandModule>();

            await this.Client.ConnectAsync();
            await this.Lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);
        }

        

        #region -- Логи --
        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            sender.Logger.LogError(BotEventId, e.Exception, "Произошел exeption");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} успешно выполнил '{e.Command.QualifiedName}'");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} попытался выполнить '{e.Command?.QualifiedName ?? "<неизвестная команда>"}' но не удалось: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Доступ заблокирован",
                    Description = $"{emoji} У тебя нет доступа для выполнения этой команды.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed);
            }
        }

        #endregion

    }
}
