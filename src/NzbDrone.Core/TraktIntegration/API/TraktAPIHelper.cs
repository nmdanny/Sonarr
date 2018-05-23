using System;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.TraktIntegration.Credentials;

namespace NzbDrone.Core.TraktIntegration.API
{
    /// <summary>
    /// Includes utility methods and constants used for Trakt.
    /// </summary>
    public class TraktAPIHelper
    {

        private readonly string API_URL = "https://api.trakt.tv";
        private readonly string API_VERSION = "2";

        private readonly IConfigFileProvider cfgFileProvider;

        public TraktAPIHelper(IConfigFileProvider cfgFileProvider)
        {
            this.cfgFileProvider = cfgFileProvider;
        }

        public HttpRequest PrepareTraktRequest(TraktCredentials creds, string path, HttpMethod method = HttpMethod.GET)
        {
            var req = new HttpRequest($"{API_URL}/{path.Trim('/')}");
            req.Headers.ContentType = "application/json";
            req.Headers.Add("trakt-api-version", API_VERSION);
            req.Headers.Add("trakt-api-key", creds.ClientId);
            req.Method = method;
            if (!String.IsNullOrEmpty(creds.AccessToken))
            {
                req.Headers.Add("Authorization", $"Bearer {creds.AccessToken}");
            }
            return req;
        }

        /// <summary>
        /// Generates a URL to redirect the client to Trakt's OAuth2 endpoint.
        /// </summary>
        public string ClientRedirectUri(OAuthState state)
        {
            var address = new HttpUri("https://trakt.tv/oauth/authorize")
                   .AddQueryParam("client_id", state.ClientId)
                   .AddQueryParam("redirect_uri", ExchangeRedirectUri())
                   .AddQueryParam("response_type", "code")
                   .AddQueryParam("state", state.ToJson());
            return address.FullUri;
        }

        /// <summary>
        /// The redirection URL from OAuth's server back to our api.
        /// </summary>
        /// <returns></returns>
        public string ExchangeRedirectUri()
        {
            var port = cfgFileProvider.Port;
            var url = cfgFileProvider.UrlBase;
            var api_key = cfgFileProvider.ApiKey;
            // TODO: find neater way of detecting the hostname, maybe from browser?
            return $"http://localhost:{port}/api/traktcredentials/oauth?apikey={api_key}";
        }

    }
}
