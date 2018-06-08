using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.TraktIntegration.API;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.TraktIntegration.Settings;

namespace NzbDrone.Core.TraktIntegration.Credentials
{
    /// <summary>
    /// <para>Manages credentials needed for Trakt's APIs. 
    /// <para>Note that you can't have more than one set of Trakt credentials at any time.</para>
    /// </summary>
    public interface ITraktCredentialsManager
    {
        /// <summary>
        /// Creates and stores Trakt credentials with the given parameters, overwriting any
        /// previous credentials if existing.
        /// </summary>
        /// <param name="state">OAuth state parameters</param>
        /// <param name="code">Code used for OAuth access code exchange</param>
        /// <returns>The newly added credentials</returns>
        TraktCredentials AddTraktCredentials(OAuthState state, string code);

        /// <summary>
        /// Obtains the currently stored credentials, throwing an error if none exist.
        /// </summary>
        TraktCredentials GetTraktCredentials();

        /// <summary>
        /// Ensures that the stored credentials are updated, otherwise refreshes them.
        /// Throws an error if no credentials exist.
        /// </summary>
        void EnsureFreshCredentialsAvailable();

        /// <summary>
        /// Deletes the stored credentials if they exist, otherwise does nothing.
        /// </summary>
        void DeleteCredentialsIfExists();

        /// <summary>
        /// Checks if Trakt credentials are available.
        /// </summary>
        bool HasCredentials { get; }

    }

    public class TraktCredentialsManager : ITraktCredentialsManager
    {
        private readonly IConfigObjectStore<TraktSettings> store;
        private readonly Logger logger;
        private readonly ITraktAPIHelper helper;
        private readonly IOAuthStateCrypto oauthStateCrypto;
        private readonly IEventAggregator eventAggregator;

        public TraktCredentialsManager(Logger logger, IConfigObjectStore<TraktSettings> store, ITraktAPIHelper helper,
            IOAuthStateCrypto oauthStateCrypto, IEventAggregator eventAggregator)
        {
            this.logger = logger;
            this.store = store;
            this.helper = helper;
            this.oauthStateCrypto = oauthStateCrypto;
            this.eventAggregator = eventAggregator;
        }

        public bool HasCredentials => store.Item?.Credentials != null;

        public TraktCredentials GetTraktCredentials()
        {
            return store.Item?.Credentials ?? throw new MissingTraktCredentials();
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
            if (store.Item == null)
            {
                store.Item = new TraktSettings();
            }
            creds = helper.FillOAuthTokens(creds, code, false);
            var validation = creds.Validate();
            if (!validation.IsValid)
            {
                var err = string.Join(", ", validation.Errors);
                throw new InvalidCredentialsException($"Trakt credentials aren't valid: {err} ");
            }
            store.Item.Credentials = creds;
            store.Save();
            eventAggregator.PublishEvent(new Events.TraktCredentialsAddedEvent());
            logger.Info("Trakt Credentials added successfully.");
            return creds;
        }

        public void DeleteCredentialsIfExists()
        {
            if (!HasCredentials)
            {
                logger.Debug("Tried to delete Trakt credentials, but none exist.");
                return;
            }

            store.Item.Credentials = null;
            store.Save();
            eventAggregator.PublishEvent(new Events.TraktCredentialsDeletedEvent());
            logger.Info("Deleted Trakt credentials successfully.");
        }

        public void EnsureFreshCredentialsAvailable()
        {
            var creds = GetTraktCredentials();
            if (DateTime.Now < creds.ExpirationDate)
            {
                logger.Debug("Trakt credentials are fresh, no need to do anything");
                return;
            }
            else
            {
                logger.Debug("Trakt OAuth2 access token expired, getting a new one.");
                store.Item.Credentials = helper.FillOAuthTokens(creds, creds.RefreshToken, true);
                store.Save();
            }

        }

    }
}
