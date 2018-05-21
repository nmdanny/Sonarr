using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API
{
    /// <summary>
    /// Wrapper class for a Trakt show and metadata about how it was sourced.
    /// </summary>
    public interface ISourcedShow
    {
        TraktSources SourceType { get; }
        Show Show { get; }
    }
}
