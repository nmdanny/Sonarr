using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Http;
using NLog;

using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.API;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials;
using System.Threading.Tasks;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{
    public interface ITraktAPIService
    {
        /// <summary>
        /// Fetches TV shows from your Trakt account.
        /// </summary>
        /// <param name="sources">From where in your account shall we fetch shows?(Watchlist, Collection, Watched, Recommended)
        ///                       Note, this is a bitfield.</param>
        /// <param name="recommendedAmount">If looking for recommended shows, how many of them?</param>
        /// <returns>Contains the show along with its Trakt source</returns>
        IEnumerable<ISourcedShow> GetTraktShows(TraktSources sources, int recommendedAmount = 10);

        /// <summary>
        /// Filters the given list of shows, fetching only shows that are 'fresh', aka, contain an unseen episode.
        /// </summary>
        /// <param name="interestedShows">Shows to filter</param>
        /// <param name="includeSpecials">Should we include special episodes?</param>
        /// <returns>The fresh show tupled with the unseen(but aired) episode</returns>
        IEnumerable<Tuple<Show, Episode>> GetFreshShows(IEnumerable<Show> interestedShows, bool includeSpecials);

        /// <summary>
        /// Fetches progress information for a certain show
        /// </summary>
        /// <param name="traktId">Trakt ID/Slug/IMDB ID</param>
        /// <param name="includeSpecials">Should we include special episodes?</param>
        /// <returns>Progress includes last watch date, next episode to see(if it aired) among other information.</returns>
        WatchedProgress FetchWatchedProgress(string traktId, bool includeSpecials);

    }
    public class TraktAPIService: ITraktAPIService
    {
        private readonly ITraktCredentialsStore credStore;
        private readonly IHttpClient http;
        private readonly Logger logger;

        private readonly TraktCredentials credentials;
        private readonly string API_URL = "https://api.trakt.tv";
        private readonly string API_VERSION = "2";


        public TraktAPIService(ITraktCredentialsStore credStore, IHttpClient http, Logger logger)
        {
            this.credStore = credStore;
            this.http = http;
            credStore.EnsureFreshCredentialsAvailable();
            this.credentials = credStore.GetTraktCredentials();
            this.logger = logger;
        }
        private HttpRequest PrepareTraktRequest(TraktCredentials creds, string path, HttpMethod method = HttpMethod.GET)
        {
            var k = new HttpRequestBuilder(API_URL);
            var req = new HttpRequest($"{API_URL}/{path.Trim('/')}");
            req.Headers.ContentType = "application/json";
            req.Headers.Add("trakt-api-version", API_VERSION);
            req.Headers.Add("trakt-api-key", creds.ClientId);
            req.Headers.Add("Authorization", $"Bearer {creds.AccessToken}");
            req.Method = method;
            return req;
        }
        private IEnumerable<WatchlistShow> FetchWatchlistShows()
        {
            var req = PrepareTraktRequest(credentials, "/users/me/watchlist/shows");
            return http.Get<List<WatchlistShow>>(req).Resource;
        }

        private IEnumerable<WatchedShow> FetchWatchedShows()
        {
            var req = PrepareTraktRequest(credentials, "/users/me/watched/shows");
            return http.Get<List<WatchedShow>>(req).Resource;
        }
        private IEnumerable<CollectedShow> FetchCollectedShows()
        {
            var req = PrepareTraktRequest(credentials, "/users/me/collection/shows");
            return http.Get<List<CollectedShow>>(req).Resource;
        }

        private IEnumerable<RecommendedShow> FetchRecommendedShows(int count)
        {
            var req = PrepareTraktRequest(credentials, $"/recommendations/shows?limit={count}");
            return http.Get<List<RecommendedShow>>(req).Resource;

        }

        public WatchedProgress FetchWatchedProgress(string traktId, bool includeSpecials)
        {
            var req = PrepareTraktRequest(credentials, $"/shows/{traktId}/progress/watched");
            req.Url.AddQueryParam("specials", includeSpecials);
            return http.Get<WatchedProgress>(req).Resource;
        }

        public IEnumerable<ISourcedShow> GetTraktShows(TraktSources sources, int recommendedAmount = 10)
        {
            logger.Debug($"Trakt service is getting a list of shows from the following sources: {sources}");
            var shows = new List<ISourcedShow>();
            if (sources.HasFlag(TraktSources.Watchlist))
            {
                shows.AddRange(FetchWatchlistShows());
            }
            if (sources.HasFlag(TraktSources.Watched))
            {
                shows.AddRange(FetchWatchedShows());
            }
            if (sources.HasFlag(TraktSources.Collection))
            {
                shows.AddRange(FetchCollectedShows());
            }
            if (sources.HasFlag(TraktSources.Recommended))
            {
                shows.AddRange(FetchRecommendedShows(recommendedAmount));
            }
            logger.Debug($"Trakt service successfully fetched a list of {shows.Count} shows");
            return shows;
        }

        public IEnumerable<Tuple<Show, Episode>> GetFreshShows(IEnumerable<Show> interestedShows, bool includeSpecials)
        {
            var fresh = new System.Collections.Concurrent.ConcurrentBag<Tuple<Show, Episode>>();
            Parallel.ForEach(interestedShows, show =>
            {
                var watchProgress = FetchWatchedProgress(show.Ids.Trakt.ToString(), includeSpecials);
                if (watchProgress.NextEpisode != null)
                {
                    fresh.Add(Tuple.Create(show, watchProgress.NextEpisode));
                }
            });
            logger.Debug($"Trakt service found a list of {fresh.Count} fresh shows/episodes to watch");
            return fresh;
        }


    }
}
