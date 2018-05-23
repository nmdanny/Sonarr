using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    public class WatchedProgress
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Aired { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Completed { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public DateTime LastWatchedAt { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public object Seasons { get; set; } // don't need this

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public object HiddenSeasons { get; set; } // nor this

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public Episode NextEpisode { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public Episode LastEpisode { get; set; }
    }
}
