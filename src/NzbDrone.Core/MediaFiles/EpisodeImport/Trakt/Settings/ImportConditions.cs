namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings
{
    /// <summary>
    /// What kind of shows shall be automatically imported via Trakt?
    /// </summary>
    public enum ImportConditions
    {
        /// <summary>
        /// Only shows that exist in your account(as per <see cref="TraktSources"/>) that have unwatched episodes, aka 'fresh'
        /// </summary>
        UnseenEpisodes,
        /// <summary>
        /// All seasons that exist in your account(as per <see cref="TraktSources"/>), including those that you've entirely watched
        /// Not recommended unless you like to hoard shows.
        /// </summary>
        All
    }
}
