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

namespace NzbDrone.Core.Test.TraktIntegrationTests
{
    [TestFixture]
    public class OAuthStateCryptoFixture: CoreTest<OAuthStateCrypto>
    {
        [Test]
        public void verifies_valid_state_successfully()
        {
            var state = Builder<OAuthState>.CreateNew().Build();
            Subject.Sign(state);
            Assert.True(Subject.Verify(state));
        }

        [Test]
        public void fails_verification_on_tampered_state()
        {
            var state = Builder<OAuthState>.CreateNew().Build();
            Subject.Sign(state);
            state.RedirectTo = "malicious-site.com";
            Assert.False(Subject.Verify(state));
        }
    }
}
