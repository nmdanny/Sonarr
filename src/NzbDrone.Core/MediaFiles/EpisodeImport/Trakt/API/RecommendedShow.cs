using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
{
    public class RecommendedShow : Show, ISourcedShow
    {
        public TraktSources SourceType => TraktSources.Recommended;

        public Show Show => this;
    }
}
