using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
{
    public class WatchlistShow: ISourcedShow
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Rank { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public DateTime ListedAt { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string Type { get => "show"; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public Show Show { get; set; }

        public TraktSources SourceType => TraktSources.Watchlist;

    }
}
