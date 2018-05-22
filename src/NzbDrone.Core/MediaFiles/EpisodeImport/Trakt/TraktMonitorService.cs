using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Settings;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{
    public class TraktMonitorService : IExecute<TraktMonitorCommand>
    {
        private readonly ISeriesService seriesService;
        private readonly ITraktAPIService traktService;
        private readonly IEpisodeService episodeService;
        private readonly TraktAPIConverter traktAPIConverter;
        private readonly Logger logger;

        public TraktMonitorService(ISeriesService seriesService, ITraktAPIService traktService, IEpisodeService episodeService, TraktAPIConverter traktAPIConverter, Logger logger)
        {
            this.seriesService = seriesService;
            this.traktService = traktService;
            this.episodeService = episodeService;
            this.traktAPIConverter = traktAPIConverter;
            this.logger = logger;
        }

        private List<Series> GetSeriesToManage(TraktSources monitorSources, MonitorBehavior behavior)
        {
            if (behavior == MonitorBehavior.TraktOnly)
            {
                throw new NotImplementedException("TODO Handle TraktOnly for GetSeriesToMonitor");
            }
            else
            {
                return seriesService.GetAllSeries()
                    .IntersectBy(s => s.TvdbId, traktService.GetTraktShows(monitorSources), s => s.Show.Ids.Tvdb,
                                       EqualityComparer<int>.Default)
                    .ToList();
            }

        }

        private void ApplyMonitoringRules(Series series, Episode unseenEpisode, int maxMonitorings, bool includeFollowingSeasons)
        {
            var allEpisodes = episodeService.GetEpisodeBySeries(series.Id);

            var seenEpisodes = allEpisodes.Where(e =>  e.SeasonNumber  < unseenEpisode.SeasonNumber
                                                   || (e.SeasonNumber == unseenEpisode.SeasonNumber &&
                                                       e.EpisodeNumber < unseenEpisode.EpisodeNumber));

            var toMonitor = allEpisodes.Except(seenEpisodes)
                                       .Where(e => includeFollowingSeasons ? true : e.SeasonNumber == unseenEpisode.SeasonNumber)
                                       .Take(maxMonitorings);

            logger.Debug($"Out of {allEpisodes.Count()} episodes for {series}, we have seen(unmonitor) {seenEpisodes.Count()} episodes and " +
                         $"have to monitor a total of {toMonitor.Count()} episodes");
            foreach (var seen in seenEpisodes)
            {
                episodeService.SetEpisodeMonitored(seen.Id, false);
            }
            foreach (var toSee in toMonitor)
            {
                episodeService.SetEpisodeMonitored(toSee.Id, true);
            }
            
        }

        public void Execute(TraktMonitorCommand message)
        {
            logger.Info($"Beginning monitor management");
            var showsToManage = GetSeriesToManage(message.AllMonitoringSources, message.MonitorBehavior);
            logger.Debug($"Monitor management behavior: {message.MonitorBehavior}, sources: {message.AllMonitoringSources}");
            logger.Debug($"Found a total of {showsToManage.Count} shows that require monitor management");
            foreach (var series in showsToManage)
            {
                logger.Debug($"Managing {series}");
                if (string.IsNullOrEmpty(series.ImdbId))
                {
                    // TODO find alternative for this issue
                    logger.Warn($"Series '{series}' has no ImdbId, can't find its progress in Trakt");
                    continue;
                }
                var watchProgres = traktService.FetchWatchedProgress(series.ImdbId, message.IncludeSpecials);
                if (watchProgres.NextEpisode == null)
                {
                    logger.Debug($"No new episodes to see, skipping...");
                    continue;
                }
                var unseenEpisode = traktAPIConverter.TraktEpisodeToModel(watchProgres.NextEpisode, series);
                logger.Debug($"New episode is {unseenEpisode}, max number of monitored episodes: {message.MaxMonitorPerSeries}, " +
                             $"Continuing beyond the unseen season: {message.MonitorFollowingSeasons}");
                ApplyMonitoringRules(series, unseenEpisode, message.MaxMonitorPerSeries, message.MonitorFollowingSeasons);
                logger.Debug($"Finished handling {series}");
            }
            logger.Info($"Finished setting episode monitored statuses.");
        }

    }
}
