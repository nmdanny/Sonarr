using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Core.TraktIntegration.Settings;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    public class CollectedShow : ISourcedShow
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public DateTime CollectedAt { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public Show Show { get; set; }

        public TraktSources SourceType => TraktSources.Collection;

    }
}
