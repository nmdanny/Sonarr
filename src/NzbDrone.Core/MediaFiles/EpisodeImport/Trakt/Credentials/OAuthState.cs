using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Composition;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials
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
    }
}
