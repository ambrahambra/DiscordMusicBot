using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Bot.Modules
{
    public class MusicCommandModule : BaseCommandModule
    {
        private MusicPlayer MusicPlayer { get; set; }

        [Command("join"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (chn == null)
                chn = vstat.Channel;


            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            if (chn.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            MusicPlayer = new MusicPlayer(chn.Guild, lava);
            await MusicPlayer.CreatePlayerAsync(chn);
            await ctx.RespondAsync($"Joined {chn.Name}!");
        }

        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx, DiscordChannel chn = null)
        {
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (chn == null)
                chn = vstat.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (chn.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(chn.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }
            
            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            await MusicPlayer.DestroyPlayerAsync();
            await ctx.RespondAsync($"Left {chn.Name}!");
        }

        [Command("play"), Description("Search and play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            Song song = new Song
            {
                Track = loadResult.Tracks.First(),
                RequestedBy = ctx.Member
            };
            
            MusicPlayer.Enqueue(song);
            if (MusicPlayer.IsPlaying)
            {
                await ctx.RespondAsync($"{song.Track.Title} add to queue.");
            }
            else
            {
                await MusicPlayer.PlayAsync();
                await ctx.RespondAsync($"Now playing {song.Track.Title}!");
            }
        }



        // [Command("play"), Description("Plays audio on url")]
        // public async Task Play(CommandContext ctx, Uri url)
        // {
        //     if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        //     {
        //         await ctx.RespondAsync("You are not in a voice channel.");
        //         return;
        //     }
        //
        //     var lava = ctx.Client.GetLavalink();
        //     var node = lava.ConnectedNodes.Values.First();
        //     var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
        //
        //     if (conn == null)
        //     {
        //         await ctx.RespondAsync("Lavalink is not connected.");
        //         return;
        //     }
        //
        //     var loadResult = await node.Rest.GetTracksAsync(url);
        //
        //     if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
        //         || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        //     {
        //         await ctx.RespondAsync($"Track search failed for {url}.");
        //         return;
        //     }
        //
        //     var track = loadResult.Tracks.First();
        //
        //     await conn.PlayAsync(track);
        //
        //
        //     await ctx.RespondAsync($"Now playing {track.Title}!");
        // }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.NowPlaying.Track.TrackString is null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await MusicPlayer.PauseAsync();
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.NowPlaying.Track.TrackString is null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            
            await MusicPlayer.ResumeAsync();
        }

        [Command]
        public async Task Stop(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.NowPlaying.Track.TrackString is null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            MusicPlayer.EmptyQueue();
            await MusicPlayer.StopAsync();
        }

        [Command]
        public async Task Skip(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.NowPlaying.Track.TrackString is null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            
            var currentSong = MusicPlayer.NowPlaying;
            await MusicPlayer.StopAsync();
            await ctx.RespondAsync($"Skip: {currentSong.Track.Title}");
            
        }

        [Command]
        public async Task Now(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.NowPlaying.Track.TrackString is null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await ctx.RespondAsync($"Now playing: {MusicPlayer.NowPlaying.Track.Title}");
        }

        [Command]
        public async Task ClearAll(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.Queue.Count == 0)
            {
                await ctx.RespondAsync("Queue is empty");
                return;
            }

            var count = MusicPlayer.EmptyQueue();
            await ctx.RespondAsync($"{count} item(s) was cleared");
        }

        [Command]
        public async Task ShowQueue(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.Queue.Count == 0)
            {
                await ctx.RespondAsync("Queue is empty");
                return;
            }

            var sb = new StringBuilder();
            var queue = MusicPlayer.Queue;
            int point = 0;
            foreach (var song in queue)
            {
                sb.Append($"[{point}] {song} \n");
                point++;
            }

            await ctx.RespondAsync(sb.ToString());
        }

        [Command]
        public async Task Remove(CommandContext ctx, int id)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (MusicPlayer is null)
            {
                await ctx.RespondAsync("Music player not working.");
                return;
            }

            if (MusicPlayer.Queue.Count == 0)
            {
                await ctx.RespondAsync("Queue is empty.");
                return;
            }

            if (id > MusicPlayer.Queue.Count || id < 0)
            {
                await ctx.RespondAsync("Number out of range.");
                return;
            }

            var song = MusicPlayer.Remove(id);
            await ctx.RespondAsync($"{song.Track.Title} was removed.");
        }
    }
}
