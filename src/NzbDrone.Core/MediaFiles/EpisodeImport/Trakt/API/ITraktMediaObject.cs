using System;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
{
    /// <summary>
    /// https://trakt.docs.apiary.io/#introduction/standard-media-objects
    /// </summary>
    public interface ITraktMediaObject
    {
        TraktIDs Ids { get; }
    }
}
