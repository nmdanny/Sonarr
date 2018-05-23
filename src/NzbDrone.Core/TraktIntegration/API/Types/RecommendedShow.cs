using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.TraktIntegration.Settings;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    public class RecommendedShow : Show, ISourcedShow
    {
        public TraktSources SourceType => TraktSources.Recommended;

        public Show Show => this;
    }
}
