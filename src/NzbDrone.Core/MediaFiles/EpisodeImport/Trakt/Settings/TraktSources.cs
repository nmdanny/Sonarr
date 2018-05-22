using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings
{
    /// <summary>
    /// What kind of Trakt sources shall a certain behavior apply to?
    /// Note, this enum is a bitfield.
    /// </summary>
    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum TraktSources
    {
        /// <summary>
        /// None, effectively disabling the given behavior
        /// </summary>
        None = 0,
        /// <summary>
        /// Shows that are recommended to you
        /// </summary>
        Recommended = 1,
        /// <summary>
        /// Any show that you've ever watched
        /// </summary>
        Watched = 2,
        /// <summary>
        /// Any show that exists in your watchlist
        /// </summary>
        Watchlist = 4,
        /// <summary>
        /// Any show that you collected
        /// </summary>
        Collection = 8,
        /// <summary>
        /// Combination of above
        /// </summary>
        All = 1 | 2 | 4 | 8
    }
}
