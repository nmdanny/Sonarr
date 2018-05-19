using System;
using FluentValidation;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles.EpisodeImport.Trakt;

namespace NzbDrone.Api.Trakt
{
    public class TraktCredentialsResource: RestResource
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Username { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public DateTime? LastRefreshDate { get; set; }

    }

    public class TraktCredentialsResourceValidator : ResourceValidator<TraktCredentialsResource>
    {
        public TraktCredentialsResourceValidator()
        {
            RuleFor(creds => creds.ClientId).NotEmpty();
            RuleFor(creds => creds.Secret).NotEmpty();
            RuleFor(creds => creds.Username).NotEmpty();

            When(creds => !String.IsNullOrEmpty(creds.AccessToken), () =>
            {
                RuleFor(creds => creds.AccessToken).NotEmpty();
                RuleFor(creds => creds.RefreshToken).NotEmpty();
                RuleFor(creds => creds.LastRefreshDate).NotNull().LessThanOrEqualTo(DateTime.Now);
            });
        }
    }

    public static class TraktCredentialsMapper
    {
        public static TraktCredentials ToModel(this TraktCredentialsResource creds)
        {
            return new TraktCredentials()
            {
                ClientId = creds.ClientId,
                Secret = creds.Secret,
                Username = creds.Username,
                AccessToken = creds.AccessToken,
                RefreshToken = creds.RefreshToken,
                LastRefreshDate = creds.LastRefreshDate
            };
        }
        public static TraktCredentialsResource ToResource(this TraktCredentials creds)
        {
            return new TraktCredentialsResource()
            {
                ClientId = creds.ClientId,
                Secret = creds.Secret,
                Username = creds.Username,
                AccessToken = creds.AccessToken,
                RefreshToken = creds.RefreshToken,
                LastRefreshDate = creds.LastRefreshDate
            };
        }
    }
}
