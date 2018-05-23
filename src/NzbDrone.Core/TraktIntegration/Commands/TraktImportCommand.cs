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

        public ImportConditions ImportConditions { get; set; }
        public TraktSources ImportFrom;
        public bool IncludeSpecials { get; set; }
        public string RootFolderPath { get; set; }
        public int ProfileId { get; set; }

    }
}
