using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{
    public class TraktImportService : IExecute<TraktImportCommand>
    {
        private readonly ISeriesService seriesService;
        private readonly IAddSeriesService addSeriesService;
        private readonly ITraktAPIService traktService;
        private readonly Logger logger;
        public TraktImportService(ISeriesService seriesService, ITraktAPIService traktService,
                                  IAddSeriesService addSeriesService, Logger logger)
        {
            this.seriesService = seriesService;
            this.logger = logger;
            this.traktService = traktService;
            this.addSeriesService = addSeriesService;
        }
        public void Execute(TraktImportCommand message)
        {
            logger.Info($"Importing shows from the following Trakt sources: {message.ImportFrom}, conditions: {message.ImportConditions}");
            var showsFromTrakt = traktService.GetTraktShows(message.ImportFrom)
                .Select(s => s.Show)
                .ExceptBy(s => s.Ids.Tvdb, seriesService.GetAllSeries(), s => s.TvdbId, EqualityComparer<int>.Default)
                .ToList();
            logger.Debug($"Found {showsFromTrakt.Count} Trakt shows that aren't in Sonarr.");
            IEnumerable<Tuple<API.Show, API.Episode>> showsToImport;
            if (message.ImportConditions == ImportConditions.UnseenEpisodes)
            {
                showsToImport = traktService.GetFreshShows(showsFromTrakt, message.IncludeSpecials);
            } else
            {
                showsToImport = showsFromTrakt.Select(show => Tuple.Create<API.Show, API.Episode>(show, null));
            }

            logger.Debug($"Importing {showsToImport.Count()} shows from Trakt");
            foreach (var kvp in showsToImport)
            {
                var addedSeries = addSeriesService.AddSeries(new Series()
                {
                    TvdbId = kvp.Item1.Ids.Tvdb,
                    Monitored = false, // this will be handled by a different service
                    RootFolderPath = message.RootFolderPath,
                    SeasonFolder = true,
                    ProfileId = message.ProfileId
                });
                
            }
            logger.Info("Import done");
        }
        

    }
}
