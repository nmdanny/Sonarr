using Nancy;
using NLog;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.TraktIntegration.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Trakt
{
    public class TraktSettingsModule: NzbDroneRestModule<TraktSettingsResource>
    {
        private readonly Logger logger;
        private readonly TraktSettingsStore store;

        public TraktSettingsModule(Logger logger, TraktSettingsStore store, TraktSettingsValidator settingsValidator)
        {
            this.logger = logger;
            this.store = store;

            CreateResource = CreateSettings;
            UpdateResource = SetSettings;
            GetResourceSingle = GetSettings;
            Delete["/"] = x => { DeleteSettings(1); return new object().AsResponse(); };
            SharedValidator.Include(settingsValidator);
        }

        private void SetSettings(TraktSettingsResource obj)
        {
            obj.UpdateModel(store.Item);
            store.Save();
        }

        private int CreateSettings(TraktSettingsResource obj)
        {
            var settings = new TraktSettings();
            obj.UpdateModel(settings);
            store.Item = settings;
            return 1; // id doesnt matter

        }

        private TraktSettingsResource GetSettings()
        {
            return store.Item != null ? TraktSettingsResource.FromModel(store.Item) : null;
        }

        private void DeleteSettings(int obj)
        {
            store.Erase();
        }
    }
}
