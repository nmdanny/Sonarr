using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.TraktIntegration.Commands;
using NzbDrone.Core.TraktIntegration.Settings;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.TraktIntegration.API.Types;

namespace NzbDrone.Core.TraktIntegration
{
    public class TraktImportService : IExecute<TraktImportCommand>
    {
        private readonly ISeriesService seriesService;
        private readonly IAddSeriesService addSeriesService;
        private readonly ITraktAPIService traktService;
        private readonly IManageCommandQueue cmdQueue;
        private readonly Logger logger;

        public TraktImportService(ISeriesService seriesService, ITraktAPIService traktService,
                                  IAddSeriesService addSeriesService, Logger logger, IManageCommandQueue cmdQueue)
        {
            this.seriesService = seriesService;
            this.logger = logger;
            this.traktService = traktService;
            this.addSeriesService = addSeriesService;
            this.cmdQueue = cmdQueue;
        }

        public void Execute(TraktImportCommand message)
        {
            logger.Info($"Importing shows from the following Trakt sources: {message.ImportFrom}, conditions: {message.ImportConditions}");
            List<Show> missingShows = GetMissingTraktShows(message.ImportFrom);
            logger.Debug($"Found {missingShows.Count} Trakt shows that aren't in Sonarr.");
            var showsToImport = FilterShowsToImport(missingShows, message.ImportConditions, message.IncludeSpecials);
            logger.Debug($"Importing {showsToImport.Count()} shows from Trakt");
            foreach (var show in showsToImport)
            {
                ImportShow(show, message.RootFolderPath, message.ProfileId);
                logger.Debug($"Imported {show}");
            }
            logger.Info("Import done");
            cmdQueue.Push(new TraktMonitorCommand()
            {
                AllMonitoringSources = message.ImportFrom,
                IncludeSpecials = message.IncludeSpecials,
                MonitorBehavior = MonitorBehavior.All,
                MonitorFollowingSeasons = true,
                MaxMonitorPerSeries = Int32.MaxValue
            }, priority: CommandPriority.High);
        }

        private IEnumerable<Show> FilterShowsToImport(List<Show> missingTraktShows,
                                                          ImportConditions importConditions, bool includeSpecials)
        {
            if (importConditions == ImportConditions.UnseenEpisodes)
            {
                return traktService.GetFreshShows(missingTraktShows, includeSpecials).Select(kvp => kvp.Item1);
            }
            else
            {
                return missingTraktShows;
            }

        }

        private List<Show> GetMissingTraktShows(TraktSources importFrom)
        {
            return traktService.GetTraktShows(importFrom)
                            .Select(s => s.Show)
                            .ExceptBy(s => s.Ids.Tvdb, seriesService.GetAllSeries(), s => s.TvdbId, EqualityComparer<int>.Default)
                            .ToList();
        }

        private void ImportShow(Show show, string rootFolderPath, int profileId)
        {
            var addedSeries = addSeriesService.AddSeries(new Series()
            {
                TvdbId = show.Ids.Tvdb,
                Monitored = false, // this will be handled by the TraktMonitorService
                RootFolderPath = rootFolderPath,
                SeasonFolder = true,
                ProfileId = profileId
            });
        }
    }
}
