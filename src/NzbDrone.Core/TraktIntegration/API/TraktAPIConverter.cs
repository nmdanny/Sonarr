using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.TraktIntegration;
using NzbDrone.Core.Tv;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.TraktIntegration.API
{
    /// <summary>
    /// Tries converting between Trakt API types to Sonarr model types.
    /// Important: It will only work if the relevant shows(and episodes) have been imported into Sonarr.
    /// </summary>
    public interface ITraktAPIConverter
    {
        Series TraktShowToSeries(Types.Show show);
        Episode TraktEpisodeToModel(Types.Episode episode, Series series);
        Episode TraktEpisodeToModel(Types.Episode episode, Types.Show show);
    }

    public class TraktAPIConverter : ITraktAPIConverter
    {
        private readonly ISeriesService seriesService;
        private readonly ITraktAPIService traktService;
        private readonly IEpisodeService episodeService;
        private readonly Logger logger;

        public TraktAPIConverter(ISeriesService seriesService, ITraktAPIService traktService, IEpisodeService episodeService, Logger logger)
        {
            this.seriesService = seriesService;
            this.traktService = traktService;
            this.episodeService = episodeService;
            this.logger = logger;
        }

        public Series TraktShowToSeries(Types.Show show)
        {
            return seriesService.FindByTvdbId(show.Ids.Tvdb) ??
                   seriesService.FindByTitle(show.Title) ??
                   seriesService.FindByTitleInexact(show.Title) ??
            throw new TraktException($"Couldn't find Sonarr series model for Trakt show {show}");

        }

        public Episode TraktEpisodeToModel(Types.Episode episode, Series series)
        {
            var candidates =
                   new List<Episode>() { episodeService.FindEpisode(series.Id, episode.Season, episode.Number) } ??
                   new List<Episode>() { episodeService.FindEpisodeByTitle(series.Id, episode.Season, episode.Title) } ??
                   episodeService.FindEpisodesBySceneNumbering(series.Id, episode.Season, episode.Number);
            if (candidates.Count == 0)
            {
                throw new TraktException($"Couldn't find a Sonarr Episode for Trakt Episode '{episode}' of Series '{series}'");
            }
            else if (candidates.Count > 1)
            {
                logger.Warn($"Found {candidates.Count} candidates for Trakt Episode '{episode}' of Series '{series}'");
                logger.Warn($"They are: {string.Join(", ", candidates)}");
                logger.Warn($"Using the first one: {candidates[0]}");
                return candidates[0];
            }
            else
            {
                return candidates[0];
            }

        }

        public Episode TraktEpisodeToModel(Types.Episode episode, Types.Show show)
        {
            return TraktEpisodeToModel(episode, TraktShowToSeries(show));
        }
    }

}
