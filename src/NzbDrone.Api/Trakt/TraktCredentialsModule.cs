
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using NzbDrone.Core.TraktIntegration.Credentials;
using NzbDrone.Api.Extensions;
using NLog;
using NzbDrone.Api.REST;
using NzbDrone.Core.TraktIntegration;
using NzbDrone.Core.TraktIntegration.API;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.Http;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Trakt
{
    /// <summary>
    /// REST interface for managing credentials of the Trakt API.
    /// Note, there's either 0 or 1 set of credentials stored, so IDs don't matter.
    /// </summary>
    public class TraktCredentialsModule : NzbDroneRestModuleWithSignalR<TraktCredentialsResource, TraktCredentials>
    {
        private readonly ITraktCredentialsManager credsManager;
        private readonly Logger logger;
        private readonly OAuthStateCrypto oauthStateCrypto;
        private readonly TraktAPIHelper traktAPIHelper;
        public TraktCredentialsModule(ITraktCredentialsManager credsManager, Logger logger, TraktCredentialsResourceValidator validator,
            OAuthStateCrypto oauthStateCrypto, TraktAPIHelper traktAPIHelper, IBroadcastSignalRMessage signalRBroadcaster) : base(signalRBroadcaster)
        {
            this.credsManager = credsManager;
            this.logger = logger;
            this.traktAPIHelper = traktAPIHelper;
            this.oauthStateCrypto = oauthStateCrypto;

            Get["oauth/redirect"] = x => OAuthRedirect(Request.Query["clientId"], Request.Query["secret"], Request.Query["origin"]);
            Get["oauth/redirecturl"] = x => GenerateRedirectUrl(Request.Query["clientId"], Request.Query["secret"], Request.Query["origin"]);
            Get["oauth"] = x => OAuthCallback();
            GetResourceSingle = GetTraktCredentials;
            GetResourceById = id => GetTraktCredentials();
            DeleteResource = DeleteTraktCredentials;
            Delete["/"] = x => { DeleteTraktCredentials(1); return new object().AsResponse(); };

            SharedValidator.Include(validator);
        }

        public string GenerateRedirectUrl(string clientId, string secret, string originUrl)
        {
            logger.Debug("Generating OAuth redirect URL for client");
            var oauthState = new OAuthState()
            {
                ClientId = clientId,
                Secret = secret,
                RedirectTo = originUrl
            };
            oauthStateCrypto.Sign(oauthState);
            var redirectUri = traktAPIHelper.ClientRedirectUri(oauthState);
            return redirectUri;
        }

        public object OAuthRedirect(string clientId, string secret, string originUrl)
        {
            logger.Debug("Redirecting client to OAuth server");
            var redirectUri = GenerateRedirectUrl(clientId, secret, originUrl);
            var res = Response.AsResponse(Nancy.HttpStatusCode.Found);
            res.Headers.Add(new KeyValuePair<string, string>("Location", redirectUri));
            return res;
        }

        public object OAuthCallback()
        {
            logger.Debug("Handling callback from OAuth server");
            var code = (string)Request.Query["code"];
            var state = Json.Deserialize<OAuthState>((string)Request.Query["state"]);
            credsManager.AddTraktCredentials(state, code);
            var res = Response.AsResponse(Nancy.HttpStatusCode.Found);
            res.Headers.Add(new KeyValuePair<string, string>("Location", state.RedirectTo));
            logger.Debug("Callback handled successfully, redirecting authenticating client back to " + state.RedirectTo);
            return res;
        }

        public TraktCredentialsResource GetTraktCredentials()
        {
            if (!credsManager.HasCredentials)
            {
                return new TraktCredentialsResource()
                {

                };
            }
            var model = credsManager.GetTraktCredentials();
            return model.ToResource();
        }


        // ID doesn't matter
        private void DeleteTraktCredentials(int obj)
        {
            credsManager.DeleteExistingCredentials();
        }
    }
}
