using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
{
    /// <summary>
    /// Identifiers of media objects in Trakt API.
    /// </summary>
    public class TraktIDs
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Trakt { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string Slug { get; set; } // doesnt exist for 'Episode'

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Tvdb { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string Imdb { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Tmdb { get; set; }

    }
}
