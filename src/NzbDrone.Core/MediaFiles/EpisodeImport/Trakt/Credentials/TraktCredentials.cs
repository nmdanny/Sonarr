using System;
using System.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation;
using FluentValidation;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Annotations;
using FluentValidation.Results;
using System.Collections.Generic;
using NLog;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt.Credentials
{
    /// <summary>
    /// Credentials for using the Trakt API
    /// </summary>
    public class TraktCredentials : IProviderConfig
    { 
        // Credentials

        [FieldDefinition(0, Label = "Client ID")]
        public string ClientId { get; set; }
        [FieldDefinition(1, Label = "Client Secret")]
        public string Secret { get; set; }
        [FieldDefinition(2, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "OAuth Access Token", Advanced = true)]
        public string AccessToken { get; set; }
        [FieldDefinition(4, Label = "OAuth Refresh Token", Advanced = true)]
        public string RefreshToken { get; set; }

        public DateTime? LastRefreshDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // Settings

        public TraktSources MonitorSources { get; set; } = TraktSources.All;
        public MonitorBehavior MonitorBehavior { get; set; } = MonitorBehavior.TraktOnly;
        public TraktSources ImportSources { get; set; } = TraktSources.Watched;
        public ImportConditions ImportConditions { get; set; } = ImportConditions.UnseenEpisodes;

        public static readonly TraktCredentialsValidator Validator = new TraktCredentialsValidator();

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
    public class TraktCredentialsValidator : AbstractValidator<TraktCredentials>
    {
        public TraktCredentialsValidator()
        {
            RuleFor(creds => creds.ClientId).NotEmpty();
            RuleFor(creds => creds.Secret).NotEmpty();
            RuleFor(creds => creds.Username).NotEmpty();

            RuleFor(creds => creds.AccessToken).NotEmpty();
            RuleFor(creds => creds.RefreshToken).NotEmpty();
            RuleFor(creds => creds.LastRefreshDate).NotNull().LessThanOrEqualTo(DateTime.Now);
            RuleFor(creds => creds.ExpirationDate).NotNull().GreaterThanOrEqualTo(DateTime.Now);

        }
    }
}
