using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
{
    public class CollectedShow: ISourcedShow
    {
        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public DateTime CollectedAt { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public Show Show { get; set; }

        public TraktSources SourceType => TraktSources.Collection;

    }
}
