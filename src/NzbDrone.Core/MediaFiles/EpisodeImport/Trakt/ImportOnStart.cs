using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{

    public class ImportOnStart : IHandle<ApplicationStartedEvent>
    {
        private readonly IManageCommandQueue manageCommandQueue;
        public ImportOnStart(IManageCommandQueue manageCommandQueue)
        {
            this.manageCommandQueue = manageCommandQueue;
        }
        public void Handle(ApplicationStartedEvent message)
        {
            manageCommandQueue.Push(new Commands.TraktImportCommand(ImportConditions.UnseenEpisodes,
                TraktSources.Watched, true, "D:\\Shows", 1));

        }
    }
}
