using FluentValidation;
using NzbDrone.Api.REST;
using NzbDrone.Core.TraktIntegration.Credentials;
using NzbDrone.Core.TraktIntegration.Settings;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.Trakt
{
    public class TraktSettingsResource: RestResource
    {
        public bool TraktIntegrationEnabled { get; set; }
        public bool CredentialsSet { get; set; }

        public TraktSources MonitorSources { get; set; }
        public MonitorBehavior MonitorBehavior { get; set; }

        public TraktSources ImportSources { get; set; }
        public ImportConditions ImportConditions { get; set; }

        public string DefaultRootFolder { get; set; }
        public int DefaultProfileId { get; set; }
        public bool IncludeSpecials { get; set; }

        public void UpdateModel(TraktSettings model)
        {
            model.TraktIntegrationEnabled = TraktIntegrationEnabled;
            model.Credentials = CredentialsSet ? model.Credentials : null;
            model.MonitorSources = MonitorSources;
            model.MonitorBehavior = MonitorBehavior;
            model.ImportSources = ImportSources;
            model.ImportConditions = ImportConditions;
            model.DefaultRootFolder = DefaultRootFolder;
            model.DefaultProfileId = DefaultProfileId;
            model.IncludeSpecials = IncludeSpecials;
        }
        public static TraktSettingsResource FromModel(TraktSettings model)
        {
            return new TraktSettingsResource()
            {
                TraktIntegrationEnabled = model.TraktIntegrationEnabled,
                CredentialsSet = model.Credentials != null,
                MonitorSources = model.MonitorSources,
                MonitorBehavior = model.MonitorBehavior,
                ImportSources = model.ImportSources,
                ImportConditions = model.ImportConditions,
                DefaultRootFolder = model.DefaultRootFolder,
                DefaultProfileId = model.DefaultProfileId,
                IncludeSpecials = model.IncludeSpecials
            };
        }
    }

    public class TraktSettingsValidator : ResourceValidator<TraktSettingsResource>
    {
        public TraktSettingsValidator(RootFolderValidator rootFolderValidator, PathExistsValidator pathExistsValidator,
                               ProfileExistsValidator profileExistsValidator)
        {
            When(s => s.TraktIntegrationEnabled, () =>
            {
                RuleFor(s => s.DefaultRootFolder).Cascade(CascadeMode.StopOnFirstFailure)
                    .IsValidPath()
                    .SetValidator(pathExistsValidator)
                    .SetValidator(rootFolderValidator);
                RuleFor(s => s.DefaultProfileId).SetValidator(profileExistsValidator);
            });
        }
    }

}
