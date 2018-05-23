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
    public class TraktImportCommand : Command
    {
        public TraktImportCommand(ImportConditions importConditions, TraktSources importFrom, bool includeSpecials, string rootFolderPath, int profileId)
        {
            ImportConditions = importConditions;
            ImportFrom = importFrom;
            IncludeSpecials = includeSpecials;
            RootFolderPath = rootFolderPath ?? throw new ArgumentNullException(nameof(rootFolderPath));
            ProfileId = profileId;
        }

        /// <summary>
        /// Should we import only shows with missing episodes, or all shows?
        /// </summary>
        public ImportConditions ImportConditions { get; set; }
        /// <summary>
        /// When importing shows, which Trakt sources to use?(Watched, Watchlist, Collection, Recommended, etc...)
        /// </summary>
        public TraktSources ImportFrom;
        /// <summary>
        /// Include special episodes when considering unseen episodes?
        /// </summary>
        public bool IncludeSpecials { get; set; }
        /// <summary>
        /// The default root folder for imported shows
        /// </summary>
        public string RootFolderPath { get; set; }
        /// <summary>
        /// The default profile id for imported shows
        /// </summary>
        public int ProfileId { get; set; }

    }
}
