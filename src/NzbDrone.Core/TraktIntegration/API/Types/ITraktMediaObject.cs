using System;

namespace NzbDrone.Core.TraktIntegration.API.Types
{
    /// <summary>
    /// https://trakt.docs.apiary.io/#introduction/standard-media-objects
    /// </summary>
    public interface ITraktMediaObject
    {
        TraktIDs Ids { get; }
    }
}
