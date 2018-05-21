using FluentValidation;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{
    public class TraktSettings
    {
        public bool TraktIntegrationEnabled { get; set; }
        public TraktCredentials Credentials { get; set; }

        public TraktSources MonitorSources { get; set; } = TraktSources.All;
        public MonitorBehavior MonitorBehavior { get; set; } = MonitorBehavior.TraktOnly;

        public TraktSources ImportSources { get; set; } = TraktSources.Watched;
        public ImportConditions ImportConditions { get; set; } = ImportConditions.UnseenEpisodes;

        public string DefaultRootFolder { get; set; }
        public int DefaultProfileId { get; set; }
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



    public abstract class ObjectStore<T> where T : new()
    {
        private readonly IConfigRepository repo;

        private T _object;
        public ObjectStore(IConfigRepository repo) {
            this.repo = repo;
            Initialize();
        }
        protected abstract string ConfigKey { get; }

        public T Object
        {
            get
            {
                return _object;
            }
            set
            {
                _object = value;
                Save();
            }
        }

        /// <summary>
        /// Persists the in-memory object to database.
        /// </summary>
        public void Save()
        {
            var cfgItem = repo.Get(ConfigKey);
            Ensure.That(cfgItem).IsNotNull();
            cfgItem.Value = _object.ToJson();
            repo.Update(cfgItem);
        }


        // Initializes the object from the database, defaulting if it doesn't exist.
        private void Initialize()
        {
            var cfgItem = repo.Get(ConfigKey);
            if (cfgItem == null)
            {
                cfgItem = repo.Insert(new Config()
                {
                    Key = ConfigKey,
                    Value = new T().ToJson()
                });
            }
            _object = Json.Deserialize<T>(cfgItem.Value);

        }
    }
}
