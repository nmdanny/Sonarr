using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings
{
    /// <summary>
    /// What kind of shows do we want to be managed via Trakt?
    /// (automatically monitored/unmonitored depending on watch status)
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MonitorBehavior
    {
        /// <summary>
        /// Only shows that were imported via Trakt will have their monitor status managed by Trakt
        /// </summary>
        TraktOnly,
        /// <summary>
        /// Every Sonarr show that exists in your Trakt account(as per <see cref="TraktSources"/>) will be managed by Trakt
        /// </summary>
        All
    }
}
