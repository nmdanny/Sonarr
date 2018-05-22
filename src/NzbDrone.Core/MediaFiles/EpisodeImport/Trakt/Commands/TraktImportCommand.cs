using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings;
using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Commands
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
