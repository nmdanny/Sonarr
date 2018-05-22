using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
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
    public class WatchedSeason
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Number { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public List<WatchedEpisode> Episodes { get; set; }

    }

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
