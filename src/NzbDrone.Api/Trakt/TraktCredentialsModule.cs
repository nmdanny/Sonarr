
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using NzbDrone.Api.Extensions;
using NLog;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.Http;

namespace NzbDrone.Api.Trakt
{
    /// <summary>
    /// REST interface for managing credentials of the Trakt API.
    /// Note, there's either 0 or 1 set of credentials stored, so IDs don't matter.
    /// </summary>
    public class TraktCredentialsModule : NzbDroneRestModule<TraktCredentialsResource>
    {
        private readonly ITraktCredentialsManager credStore;
        private readonly Logger logger;
        private readonly TraktCredentialsResourceValidator validator;
        private readonly OAuthStateCrypto oauthStateCrypto;
        private readonly TraktAPIHelper traktAPIHelper;
        public TraktCredentialsModule(ITraktCredentialsManager credStore, Logger logger, TraktCredentialsResourceValidator validator,
            OAuthStateCrypto oauthStateCrypto, TraktAPIHelper traktAPIHelper) : base()
        {
            this.credStore = credStore;
            this.logger = logger;
            this.validator = validator;
            this.traktAPIHelper = traktAPIHelper;
            this.oauthStateCrypto = oauthStateCrypto;

            Get["oauth/redirect"] = x => OAuthRedirect(Request.Query["clientId"], Request.Query["secret"]);
            Get["oauth"] = x => OAuthCallback();
            GetResourceSingle = GetTraktCredentials;
            GetResourceById = id => GetTraktCredentials();
            DeleteResource = DeleteTraktCredentials;
            Delete["/"] = x => { DeleteTraktCredentials(1); return new object().AsResponse(); };

            SharedValidator.Include(validator);
        }


        public object OAuthRedirect(string clientId, string secret)
        {
            logger.Debug("Redirecting client to OAuth server");
            var oauthState = new OAuthState()
            {
                ClientId = clientId,
                Secret = secret
            };
            oauthStateCrypto.Sign(oauthState);
            var redirectUri = traktAPIHelper.ClientRedirectUri(oauthState);
            var res = Response.AsResponse(Nancy.HttpStatusCode.Found);
            res.Headers.Add(new KeyValuePair<string, string>("Location", redirectUri));
            return res;
        }

        public object OAuthCallback()
        {
            logger.Debug("Handling callback from OAuth server");
            var code = (string)Request.Query["code"];
            var state = Json.Deserialize<OAuthState>(Request.Query["state"]);
            credStore.AddTraktCredentials(state, code);
            return new object().AsResponse();
        }

        public TraktCredentialsResource GetTraktCredentials()
        {
            if (!credStore.HasCredentials)
            {
                throw new MissingTraktCredentials();
            }
            var model = credStore.GetTraktCredentials();
            return model.ToResource();
        }


        // ID doesn't matter
        private void DeleteTraktCredentials(int obj)
        {
            credStore.DeleteExistingCredentials();
        }
    }
}
