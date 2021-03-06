using FluentValidation;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.TraktIntegration.Credentials;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.TraktIntegration.Settings
{
    public class TraktSettings
    {
        public bool TraktIntegrationEnabled { get; set; } = false;
        public TraktCredentials Credentials { get; set; }

        public TraktSources MonitorSources { get; set; } = TraktSources.All;
        public MonitorBehavior MonitorBehavior { get; set; } = MonitorBehavior.TraktOnly;

        public TraktSources ImportSources { get; set; } = TraktSources.Watched & TraktSources.Watchlist;
        public ImportConditions ImportConditions { get; set; } = ImportConditions.UnseenEpisodes;

        public string DefaultRootFolder { get; set; }
        public int DefaultProfileId { get; set; }
        public bool IncludeSpecials { get; set; }

    }

    public class TraktSettingsValidator: AbstractValidator<TraktSettings>
    {
        TraktSettingsValidator(RootFolderValidator rootFolderValidator, PathExistsValidator pathExistsValidator,
                               ProfileExistsValidator profileExistsValidator, TraktCredentialsValidator credentialsValidator)
        {
            When(s => s.TraktIntegrationEnabled, () =>
            {
                RuleFor(s => s.Credentials).SetValidator(credentialsValidator);
                RuleFor(s => s.DefaultRootFolder).Cascade(CascadeMode.StopOnFirstFailure)
                    .IsValidPath()
                    .SetValidator(pathExistsValidator)
                    .SetValidator(rootFolderValidator);
                RuleFor(s => s.DefaultProfileId).SetValidator(profileExistsValidator);
            });
        }
    }
}
