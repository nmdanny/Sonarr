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
    public interface ITraktAPIHelper
    {
        /// <summary>
        /// Generates a URL to redirect the client to Trakt's OAuth2 endpoint.
        /// </summary>
        string ClientRedirectUri(OAuthState state);

        /// <summary>
        /// The redirection URL from OAuth's server back to our api.
        /// </summary>
        /// <returns></returns>
        string ExchangeRedirectUri();

        /// <summary>
        /// Prepares a Trakt API request
        /// </summary>
        /// <param name="creds">Trakt credentials</param>
        /// <param name="path">API path</param>
        /// <param name="method"></param>
        /// <returns></returns>
        HttpRequest PrepareTraktRequest(TraktCredentials creds, string path, HttpMethod method = HttpMethod.GET);

        /// <summary>
        /// Fills or refreshes a <see cref="TraktCredentials"/> object with up to date OAuth
        /// tokens.
        /// </summary>
        /// <param name="creds">The impartial or outdated credentials</param>
        /// <param name="codeOrRefresh">Exchange or refresh code</param>
        /// <param name="doRefresh">Are we performing a refresh request</param>
        /// <returns></returns>
        TraktCredentials FillOAuthTokens(TraktCredentials creds, string codeOrRefresh, bool doRefresh);
    }

    public class TraktAPIHelper : ITraktAPIHelper
    {

        private readonly string API_URL = "https://api.trakt.tv";
        private readonly string API_VERSION = "2";

        private readonly IConfigFileProvider cfgFileProvider;
        private readonly IHttpClient http;

        public TraktAPIHelper(IConfigFileProvider cfgFileProvider, IHttpClient http)
        {
            this.cfgFileProvider = cfgFileProvider;
            this.http = http;
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
        public string ClientRedirectUri(OAuthState state)
        {
            var address = new HttpUri("https://trakt.tv/oauth/authorize")
                   .AddQueryParam("client_id", state.ClientId)
                   .AddQueryParam("redirect_uri", ExchangeRedirectUri())
                   .AddQueryParam("response_type", "code")
                   .AddQueryParam("state", state.ToJson());
            return address.FullUri;
        }

        public string ExchangeRedirectUri()
        {
            var port = cfgFileProvider.Port;
            var url = cfgFileProvider.UrlBase;
            var api_key = cfgFileProvider.ApiKey;
            // TODO: find neater way of detecting the hostname, maybe from browser?
            return $"http://localhost:{port}/api/traktcredentials/oauth?apikey={api_key}";
        }

        public TraktCredentials FillOAuthTokens(TraktCredentials creds, string codeOrRefresh, bool doRefresh)
        {
            var oauthReq = PrepareTraktRequest(creds, "oauth/token", HttpMethod.POST);
            switch (doRefresh)
            {
                case true:
                    var refreshRequest = new OAuthAccessRefreshRequest()
                    {
                        ClientId = creds.ClientId,
                        ClientSecret = creds.Secret,
                        RefreshToken = codeOrRefresh,
                        RedirectUri = ExchangeRedirectUri()
                    };
                    oauthReq.SetContent(refreshRequest.ToJson());
                    break;
                case false:
                    var exchangeRequest = new OAuthCodeExchangeRequest()
                    {
                        ClientId = creds.ClientId,
                        ClientSecret = creds.Secret,
                        Code = codeOrRefresh,
                        RedirectUri = ExchangeRedirectUri()
                    };
                    oauthReq.SetContent(exchangeRequest.ToJson());
                    break;
            }
            var res = http.Execute(oauthReq);
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new TraktException("Failed to obtain/refresh OAuth access token for Trakt credentials due to unknown error",
                    new HttpException(res));
            }
            try
            {
                var tokens = Json.Deserialize<OAuthTokensResponse>(res.Content);
                creds.AccessToken = tokens.AccessToken;
                creds.RefreshToken = tokens.RefreshToken;
                var lastRefreshTime = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(tokens.CreatedAt);
                creds.LastRefreshDate = lastRefreshTime;
                creds.ExpirationDate = lastRefreshTime.AddSeconds(tokens.ExpiresIn);
                return creds;
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                throw new TraktException("Failed to deserialize OAuth response tokens", ex);
            }
        }
    }
}
