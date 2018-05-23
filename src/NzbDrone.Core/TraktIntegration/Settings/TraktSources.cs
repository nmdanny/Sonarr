using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.TraktIntegration.Settings
{
    /// <summary>
    /// A flags enum indicating which Trakt sources/lists to use.
    /// </summary>
    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum TraktSources
    {
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
