using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Bot.Models;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace Bot
{
    public class MusicPlayer
    {
        public IReadOnlyCollection<Song> Queue { get; }
        public Song NowPlaying { get; private set; } = default;
        public bool IsPlaying { get; private set; } = false;
        public DiscordChannel? Channel => this.player?.Channel;

        private readonly List<Song> queue;
        private readonly LavalinkExtension lava;
        private LavalinkGuildConnection? player;

        public MusicPlayer(DiscordGuild guild, LavalinkExtension lavalink)
        {
            this.lava = lavalink;
            this.queue = new List<Song>();
            this.Queue = new ReadOnlyCollection<Song>(this.queue);
        }
        
        public async Task PlayAsync()
        {
            if (this.player is null || !this.player.IsConnected)
                return;

            if (this.NowPlaying?.Track?.TrackString == null)
                await this.PlayHandlerAsync();
        }

        public async Task StopAsync()
        {
            if (this.player is null || !this.player.IsConnected)
                return;

            this.NowPlaying = default;
            await this.player.StopAsync();
        }

        public async Task PauseAsync()
        {
            if (this.player is null || !this.player.IsConnected)
                return;

            this.IsPlaying = false;
            await this.player.PauseAsync();
        }

        public async Task ResumeAsync()
        {
            if (this.player is null || !this.player.IsConnected)
                return;

            this.IsPlaying = true;
            await this.player.ResumeAsync();
        }
        
        public async Task Next()
        {
            if (this.player is null || !this.player.IsConnected)
                return;
            
            await this.PlayHandlerAsync();
            
        }

        public int EmptyQueue()
        {
            lock (this.queue)
            {
                int count = this.queue.Count;
                this.queue.Clear();
                return count;
            }
        }

        public void Enqueue(Song item)
        {
            lock (this.queue)
            {
                this.queue.Add(item);
            }
        }

        public Song? Dequeue()
        {
            lock (this.queue)
            {
                if (this.queue.Count == 0)
                    return null;

                Song item = this.queue[0];
                this.queue.RemoveAt(0);
                return item;

            }
        }

        public Song Remove(int index)
        {
            lock (this.queue)
            {
                Song item = this.queue[index];
                this.queue.RemoveAt(index);
                return item;
            }
        }

        public async Task CreatePlayerAsync(DiscordChannel channel)
        {
            if (this.player is {IsConnected: true} || !this.lava.ConnectedNodes.Any())
                return;

            var node = this.lava.ConnectedNodes.Values.First();
            
            this.player = await node.ConnectAsync(channel);
            
            this.player!.PlaybackFinished += this.PlaybackFinishedAsync;
        }

        public async Task DestroyPlayerAsync()
        {
            if (this.player is null)
                return;

            if (this.player.IsConnected)
                await this.player.DisconnectAsync();

            this.player = null;
        }

        private async Task PlaybackFinishedAsync(LavalinkGuildConnection con, TrackFinishEventArgs e)
        {
            await Task.Delay(500);
            this.IsPlaying = false;
            await this.PlayHandlerAsync();
        }

        private async Task PlayHandlerAsync()
        {
            Song? next = this.Dequeue();
            if (next is null || this.player is null)
            {
                this.NowPlaying = default;
                return;
            }

            Song song = (Song) next;
            this.NowPlaying = song;
            this.IsPlaying = true;
            await this.player.PlayAsync(song.Track);
        }
    }
}