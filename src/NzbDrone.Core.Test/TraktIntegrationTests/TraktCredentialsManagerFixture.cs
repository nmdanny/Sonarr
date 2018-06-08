using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit;
using FluentAssertions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.TraktIntegration;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using Moq;
using NzbDrone.Core.TraktIntegration.API.Types;
using FizzWare.NBuilder;
using NzbDrone.Core.TraktIntegration.Credentials;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.TraktIntegration.API;
using NzbDrone.Core.TraktIntegration.Settings;

namespace NzbDrone.Core.Test.TraktIntegrationTests
{
    [TestFixture]
    public class TraktCredentialsManagerFixture: CoreTest<TraktCredentialsManager>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.Setup<IOAuthStateCrypto, bool>(m => m.Verify(It.IsAny<OAuthState>()))
                  .Returns(true);
            Mocker.GetMock<IConfigObjectStore<TraktSettings>>()
                  .SetupProperty(s => s.Item);

        }

        [Test]
        public void deleting_always_works()
        {
            SetCredentials(null);
            Assert.DoesNotThrow(() => Subject.DeleteCredentialsIfExists());
        }

        [Test]
        public void throws_when_missing_credentials()
        {
            SetCredentials(null);
            Assert.Throws<MissingTraktCredentials>(() => Subject.GetTraktCredentials());
        }

        [Test]
        public void works_after_adding_valid_credentials()
        {
            Mocker.Setup<ITraktAPIHelper, TraktCredentials>(m => m.FillOAuthTokens(It.IsAny<TraktCredentials>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<TraktCredentials, string, bool>((creds, _code, _doRefresh) =>
                {
                    creds.ClientId = "someclientid";
                    creds.Secret = "somesecret";
                    creds.AccessToken = "accesstoken";
                    creds.RefreshToken = "refreshtoken";
                    creds.LastRefreshDate = DateTime.Now - TimeSpan.FromSeconds(5);
                    creds.ExpirationDate = creds.LastRefreshDate + TimeSpan.FromSeconds(60);
                    return creds;
                });

            var state = Builder<OAuthState>.CreateNew().Build();
            var code = "somecode";
            Subject.AddTraktCredentials(state, code);

            Assert.IsTrue(Subject.HasCredentials);
            Assert.DoesNotThrow(() => Subject.GetTraktCredentials());

        }

        [Test]
        public void deleting_credentials_works()
        {
            SetCredentials(new TraktCredentials());
            Assert.DoesNotThrow(() => Subject.DeleteCredentialsIfExists());
            Assert.IsFalse(Subject.HasCredentials);
        }

        [Test]
        public void doesnt_refresh_updated_tokens()
        {
            SetCredentials(new TraktCredentials()
            {
                ExpirationDate = DateTime.Now + TimeSpan.FromSeconds(60)
            });

            Subject.EnsureFreshCredentialsAvailable();
            VerifyRefreshedCredentials(Times.Never());
        }

        [Test]
        public void does_refresh_outdated_tokens()
        {
            SetCredentials(new TraktCredentials()
            {
                ExpirationDate = DateTime.Now - TimeSpan.FromSeconds(5)
            });

            Subject.EnsureFreshCredentialsAvailable();
            VerifyRefreshedCredentials(Times.Once());

        }

        // directly set the TraktCredentials in the underlying manager
        private void SetCredentials(TraktCredentials creds)
        {
            var store = Mocker.Resolve<IConfigObjectStore<TraktSettings>>();
            if (store.Item == null)
            {
                store.Item = new TraktSettings();
            }
            store.Item.Credentials = creds;
        }

        private void VerifyRefreshedCredentials(Times times)
        {
            Mocker.Verify<ITraktAPIHelper>(m =>
                m.FillOAuthTokens(It.IsAny<TraktCredentials>(), It.IsAny<string>(), true), times);
        }

    }
}
