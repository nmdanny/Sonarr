using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    public class WatchedSeason
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Number { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public List<WatchedEpisode> Episodes { get; set; }

    }
}
