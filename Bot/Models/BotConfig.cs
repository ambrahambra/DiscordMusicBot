using Newtonsoft.Json;

namespace Bot.Models
{
    public class BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }

        [JsonProperty("ipLavalink")]
        public string IPLavalink { get; private set; } = "192.168.31.26";

        [JsonProperty("portLavalink")]
        public int PortLavalink { get; private set; } = 2333;

        [JsonProperty("passLavalink")]
        public string PasswordLavalink { get; private set; } = "youshallnotpass";

        [JsonProperty("retry")]
        public int RetryAmount { get; set; } = 5;
    }
}
