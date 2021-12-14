using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Bot.Models
{
    public class Song
    {
        public LavalinkTrack Track { get; set; }
        
        public DiscordMember RequestedBy { get; set; }

        public override string ToString()
        {
            return $"{Track.Title} by {RequestedBy.Username}";
        }
    }
}