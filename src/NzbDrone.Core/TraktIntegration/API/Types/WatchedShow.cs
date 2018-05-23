using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Core.TraktIntegration.Settings;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    public class WatchedShow : ISourcedShow
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Plays { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public DateTime LastWatchedAt { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public Show Show { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public List<WatchedSeason> Seasons { get; set; }

        public TraktSources SourceType => TraktSources.Watched;

    }
}
