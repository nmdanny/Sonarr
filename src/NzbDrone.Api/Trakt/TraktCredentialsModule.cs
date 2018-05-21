
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using NzbDrone.Api.Extensions;
using NLog;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Trakt
{
    /// <summary>
    /// REST interface for managing credentials of the Trakt API.
    /// Note, there's either 0 or 1 set of credentials stored, so IDs don't matter.
    /// </summary>
    public class TraktCredentialsModule : NzbDroneRestModule<TraktCredentialsResource>
    {
        private readonly ITraktCredentialsStore credStore;
        private readonly Logger logger;
        private static readonly ResourceValidator<TraktCredentialsResource> validator = new TraktCredentialsResourceValidator();

        public TraktCredentialsModule(ITraktCredentialsStore credStore, Logger logger) : base()
        {
            this.credStore = credStore;
            this.logger = logger;

            Get["oauth"] = x => OAuthCallback();
            GetResourceSingle = GetTraktCredentials;
            GetResourceById = id => GetTraktCredentials();
            CreateResource = AddTraktCredentials;
            DeleteResource = DeleteTraktCredentials;

            SharedValidator.Include(validator);
        }


        public object OAuthCallback()
        {
            var code = (string)Request.Query["code"];
            credStore.UpdateOAuthTokens(code, false);
            return (new object()).AsResponse();
        }

        public TraktCredentialsResource GetTraktCredentials()
        {

            if (!credStore.HasCredentials)
            {
                return null;
            }
            var model = credStore.GetTraktCredentials();
            return model.ToResource();
        }

        private int AddTraktCredentials(TraktCredentialsResource creds)
        {
            credStore.AddTraktCredentials(creds.ClientId, creds.Secret, creds.Username);
            return 0; // ID doesn't matter
        }


        private void DeleteTraktCredentials(int obj)
        {
            credStore.DeleteExistingCredentials();
        }
    }
}
