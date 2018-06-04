using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Composition;

namespace NzbDrone.Core.TraktIntegration.Credentials
{
    /// <summary>
    /// State that is shuffled between our server, the client and the auth provider.
    /// </summary>
    public class OAuthState
    {
        // extra data needed for trakt's api
        public string ClientId { get; set; }
        public string Secret { get; set; }
        // security related:
        public byte[] RandomBytes { get; set; }
        public byte[] Signature { get; set; }
        // to where shall our API redirect the user after hitting the OAuth callback endpoint?
        public string RedirectTo { get; set; }
    }
}
