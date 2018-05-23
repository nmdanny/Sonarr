using NzbDrone.Core.TraktIntegration.Settings;

namespace NzbDrone.Core.TraktIntegration.API.Types
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
