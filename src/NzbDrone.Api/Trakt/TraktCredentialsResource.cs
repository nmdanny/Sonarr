using System;
using FluentValidation;
using NzbDrone.Api.REST;
using NzbDrone.Core.TraktIntegration.Credentials;

namespace NzbDrone.Api.Trakt
{
    public class TraktCredentialsResource: RestResource
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public DateTime? LastRefreshDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class TraktCredentialsResourceValidator : ResourceValidator<TraktCredentialsResource>
    {
        public TraktCredentialsResourceValidator()
        {
            RuleFor(creds => creds.ClientId).NotEmpty();
            RuleFor(creds => creds.Secret).NotEmpty();
            
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
                AccessToken = creds.AccessToken,
                RefreshToken = creds.RefreshToken,
                LastRefreshDate = creds.LastRefreshDate,
                ExpirationDate = creds.ExpirationDate

    };
        }
        public static TraktCredentialsResource ToResource(this TraktCredentials creds)
        {
            return new TraktCredentialsResource()
            {
                ClientId = creds.ClientId,
                Secret = creds.Secret,
                AccessToken = creds.AccessToken,
                RefreshToken = creds.RefreshToken,
                LastRefreshDate = creds.LastRefreshDate,
                ExpirationDate = creds.ExpirationDate
            };
        }
    }
}
