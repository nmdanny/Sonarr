using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.TraktIntegration.API;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.TraktIntegration.Credentials
{
    /// <summary>
    /// <para>Manages credentials needed for Trakt's APIs. 
    /// <para>Note that you can't have more than one set of Trakt credentials at any time.</para>
    /// </summary>
    public interface ITraktCredentialsManager
    {
        TraktCredentials AddTraktCredentials(OAuthState state, string code);
        TraktCredentials GetTraktCredentials();
        void EnsureFreshCredentialsAvailable();
        void DeleteExistingCredentials();
        bool HasCredentials { get; }

    }

    public class TraktCredentialsObjectStore : ConfigObjectStore<TraktCredentials>
    {
        public TraktCredentialsObjectStore(IConfigRepository repo, IEventAggregator aggregator) : base(repo, aggregator)
        {

        }
        protected override string ConfigKey => "trakt_credentials";
        protected override bool PublishModelEvents => true;
    }

    public class TraktCredentialsManager : ITraktCredentialsManager
    {
        private readonly IHttpClient http;
        private readonly ConfigObjectStore<TraktCredentials> store;
        private readonly Logger logger;
        private readonly TraktAPIHelper helper;
        private readonly OAuthStateCrypto oauthStateCrypto;
        private readonly IEventAggregator eventAggregator;

        public TraktCredentialsManager(IHttpClient http, Logger logger, TraktCredentialsObjectStore store, TraktAPIHelper helper,
            OAuthStateCrypto oauthStateCrypto, IEventAggregator eventAggregator)
        {
            this.http = http;
            this.logger = logger;
            this.store = store;
            this.helper = helper;
            this.oauthStateCrypto = oauthStateCrypto;
            this.eventAggregator = eventAggregator;
        }

        public bool HasCredentials => store.Item != null;

        public TraktCredentials GetTraktCredentials()
        {
            return store.Item ?? throw new MissingTraktCredentials();
        }

        public TraktCredentials AddTraktCredentials(OAuthState state, string code)
        {
            if (!oauthStateCrypto.Verify(state))
            {
                throw new InvalidCredentialsException("OAuth state is invalid");
            }
            var creds = new TraktCredentials()
            {
                ClientId = state.ClientId,
                Secret = state.Secret
            };
            store.Item = FillOAuthTokens(creds, code, false);
            var validation = creds.Validate();
            if (!validation.IsValid)
            {
                var err = string.Join(", ", validation.Errors);
                throw new InvalidCredentialsException($"Trakt credentials aren't valid: {err} ");
            }
            eventAggregator.PublishEvent(new Events.TraktCredentialsAddedEvent());
            logger.Info("Trakt Credentials added successfully.");
            return creds;
        }

        public void DeleteExistingCredentials()
        {
            if (!HasCredentials)
            {
                logger.Warn("Tried to delete Trakt credentials, but none exist.");
                return;
            }

            store.Item = null;
            eventAggregator.PublishEvent(new Events.TraktCredentialsDeletedEvent());
            logger.Info("Deleted Trakt credentials successfully.");
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
            }
            else
            {
                logger.Debug("Trakt OAuth2 access token expired, getting a new one.");
                store.Item = FillOAuthTokens(creds, creds.RefreshToken, true);
            }

        }

        private TraktCredentials FillOAuthTokens(TraktCredentials creds, string codeOrRefresh, bool doRefresh)
        {
            var oauthReq = helper.PrepareTraktRequest(creds, "oauth/token", HttpMethod.POST);
            switch (doRefresh)
            {
                case true:
                    var refreshRequest = new OAuthAccessRefreshRequest()
                    {
                        ClientId = creds.ClientId,
                        ClientSecret = creds.Secret,
                        RefreshToken = codeOrRefresh,
                        RedirectUri = helper.ExchangeRedirectUri()
                    };
                    oauthReq.SetContent(refreshRequest.ToJson());
                    break;
                case false:
                    var exchangeRequest = new OAuthCodeExchangeRequest()
                    {
                        ClientId = creds.ClientId,
                        ClientSecret = creds.Secret,
                        Code = codeOrRefresh,
                        RedirectUri = helper.ExchangeRedirectUri()
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
