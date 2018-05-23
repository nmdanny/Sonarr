using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.TraktIntegration.API.Types
{

    public class WatchedEpisode
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Number { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Plays { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public DateTime LastWatchedAt { get; set; }

    }
}
