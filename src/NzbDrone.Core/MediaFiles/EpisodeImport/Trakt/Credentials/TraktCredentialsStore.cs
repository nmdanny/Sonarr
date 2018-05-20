using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Common.Serializer;
namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials
{
    /// <summary>
    /// <para>Manages credentials used for Trakt API. In order to get fully usable credentials, call these methods in the
    /// following order:</para>
    /// <para/> 1. AddTraktCredentials
    /// <para/> 2. UpdateOAuthTokens (without it, many calls to Trakt API will fail)
    /// <para>Note that you can't have more than one set of Trakt credentials.</para>
    /// </summary>
    public interface ITraktCredentialsStore
    {
        TraktCredentials AddTraktCredentials(string clientId, string clientSecret, string userName);
        void UpdateOAuthTokens(string codeOrRefresh, bool doRefresh);
        TraktCredentials GetTraktCredentials();
        void EnsureFreshCredentialsAvailable();
        void DeleteExistingCredentials();
        bool HasCredentials { get; }


    }

    public class TraktCredentialsStore : ITraktCredentialsStore
    {
        private readonly IHttpClient http;
        private readonly IConfigRepository cfgRepo;
        private readonly IConfigFileProvider cfgFileProvider;
        private readonly Logger logger;

        private readonly string API_URL = "https://api.trakt.tv";
        private readonly string API_VERSION = "2";

        private readonly string CONFIG_KEY = "traktcredentials";

        public TraktCredentialsStore(IHttpClient http, Logger logger, IConfigRepository cfg, IConfigFileProvider cfgFileProvider)
        {
            this.http = http;
            this.logger = logger;
            this.cfgRepo = cfg;
            this.cfgFileProvider = cfgFileProvider;
        }

        public bool HasCredentials => cfgRepo.Get(CONFIG_KEY) != null;

        public TraktCredentials AddTraktCredentials(string clientId, string clientSecret, string userName)
        {
            if (HasCredentials)
            {
                throw new TraktCredentialsAlreadyExist();
            }
            var creds = new TraktCredentials() { ClientId = clientId, Secret = clientSecret, Username = userName };
            TestCredentials(creds);
            cfgRepo.Insert(new Config()
            {
                Key = CONFIG_KEY,
                Value = creds.ToJson()
            });
            logger.Info($"Added credentials for Trakt user {userName}.");
            return creds;
        }

        private void TestCredentials(TraktCredentials creds, bool useMe = false)
        {
            var req = PrepareTraktRequest(creds, $"users/{(useMe ? "me" : creds.Username)}/stats", HttpMethod.GET);
            var res = http.Execute(req);
            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialsException("Invalid OAuth access token.");
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new InvalidCredentialsException("Invalid client id");
            }
            else if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                logger.Error($"Trakt request failed due to unknown error, status code: {res.StatusCode}");
                throw new TraktException($"Unknown API error when testing Trakt's credentials", new HttpException(res));
            }
            logger.Debug($"Tested Trakt credentials of {creds.Username} successfully");
        }
        public TraktCredentials GetTraktCredentials()
        {
            var cfgItem = cfgRepo.Get(CONFIG_KEY);
            if (cfgItem == null)
            {
                throw new MissingTraktCredentials();
            }
            return Json.Deserialize<TraktCredentials>(cfgItem.Value);
        }

        public HttpRequest PrepareTraktRequest(TraktCredentials creds, string path, HttpMethod method = HttpMethod.GET)
        {
            var req = new HttpRequest($"{API_URL}/{path}");
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

        public void UpdateOAuthTokens(string codeOrRefresh, bool doRefresh)
        {
            var cfgItem = cfgRepo.Get(CONFIG_KEY);
            if (cfgItem == null)
            {
                logger.Error("Tried to obtain/refresh OAuth with non-existent Trakt credentials.");
                throw new MissingTraktCredentials();
            }
            var creds = Json.Deserialize<TraktCredentials>(cfgItem.Value);

            var oauthReq = PrepareTraktRequest(creds, "oauth/token", HttpMethod.POST);
            switch (doRefresh)
            {
                case true:
                    var refreshRequest = new OAuthAccessRefreshRequest()
                    {
                        ClientId = creds.ClientId,
                        ClientSecret = creds.Secret,
                        RefreshToken = codeOrRefresh,
                        RedirectUri = RedirectUri()
                    };
                    oauthReq.SetContent(refreshRequest.ToJson());
                    break;
                case false:
                    var exchangeRequest = new OAuthCodeExchangeRequest()
                    {
                        ClientId = creds.ClientId,
                        ClientSecret = creds.Secret,
                        Code = codeOrRefresh,
                        RedirectUri = RedirectUri()
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

                cfgItem.Value = creds.ToJson();
                cfgRepo.Update(cfgItem);
                logger.Info($"Successfully {(doRefresh ? "refreshed": "filled")} OAuth credentials for Trakt");
            } catch (Newtonsoft.Json.JsonException ex)
            {
                throw new TraktException("Failed to deserialize OAuth response tokens", ex);
            }
            TestCredentials(creds, true);


        }
        public void DeleteExistingCredentials()
        {
            if (!HasCredentials)
            {
                logger.Warn("Tried to delete Trakt credentials, but none exist.");
                return;
            }

            var item = cfgRepo.Get(CONFIG_KEY);
            cfgRepo.Delete(item);
            logger.Info("Deleted Trakt credentials successfully.");
        }

        public string RedirectUri()
        {
            var port = cfgFileProvider.Port;
            var url = cfgFileProvider.UrlBase;
            var api_key = cfgFileProvider.ApiKey;
            // TODO: find neater way of detecting the hostname
            return $"http://localhost:{port}/api/traktcredentials/oauth?apikey={api_key}";
        }

        public void EnsureFreshCredentialsAvailable()
        {
            if (!HasCredentials)
            {
                throw new MissingTraktCredentials();
            }
            var creds = GetTraktCredentials();
            if (creds.LastRefreshDate < creds.ExpirationDate)
            {
                logger.Debug("Trakt credentials are fresh, no need to do anything");
                return;
            } else
            {
                logger.Debug("Trakt OAuth2 access token expired, getting a new one.");
                UpdateOAuthTokens(creds.RefreshToken, true);
            }
    
        }
    }

}
