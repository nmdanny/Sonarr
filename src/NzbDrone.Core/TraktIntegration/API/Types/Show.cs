using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Common.Extensions;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    public class Show : ITraktMediaObject
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string Title { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string Year { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public TraktIDs Ids { get; set; }

        public override string ToString()
        {
            return $"{Title ?? "Unknown Show"}({Year ?? "Unknown year"}";
        }
    }
}
