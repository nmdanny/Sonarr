using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TraktIntegration.Settings
{
    public class TraktSettingsStore : ConfigObjectStore<TraktSettings>
    {
        public TraktSettingsStore(IConfigRepository repo, IEventAggregator eventAggregator) : base(repo, eventAggregator)
        {
        }

        protected override string ConfigKey => "trakt_integration_settings";

        protected override bool PublishModelEvents => true;
    }
}
