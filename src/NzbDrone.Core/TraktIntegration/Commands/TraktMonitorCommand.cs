using NzbDrone.Core.Configuration;
using NzbDrone.Core.TraktIntegration.Credentials;
using NzbDrone.Core.TraktIntegration.Settings;
using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.TraktIntegration.Commands
{
    public class TraktMonitorCommand : Command
    {

        /// <summary>
        /// Monitor shows explicitly added by Trakt Integration, or all Sonarr shows with a Trakt counterpart?(as per 'AllMonitoringSources')
        /// </summary>
        public MonitorBehavior MonitorBehavior { get; set; }
        /// <summary>
        /// Which Trakt sources to consider when using the 'All' behavior?
        /// </summary>
        public TraktSources AllMonitoringSources { get; set; }
        /// <summary>
        /// The maximal amount of episodes to monitor in a series, including the first unseen episode. '0' will effectively disable this feature.
        /// </summary>
        public int MaxMonitorPerSeries { get; set; }
        /// <summary>
        /// Should you monitor seasons following the currently followed season? 
        /// </summary>
        public bool MonitorFollowingSeasons { get; set; }

        /// <summary>
        /// Include special episodes?
        /// </summary>
        public bool IncludeSpecials { get; set; }

    }
}
