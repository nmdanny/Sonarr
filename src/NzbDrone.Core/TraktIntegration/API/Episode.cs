using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.TraktIntegration.API
{
    public class Episode : ITraktMediaObject
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string Title { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Season { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public int Number { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public TraktIDs Ids { get; set; }

        public override string ToString()
        {
            return $"{Title ?? "Unknown Episode"} - S{Season}E{Number}";
        }
    }
}
