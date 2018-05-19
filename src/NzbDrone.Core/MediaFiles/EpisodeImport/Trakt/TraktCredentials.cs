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
using Newtonsoft.Json;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{
    /// <summary>
    /// Credentials for using the Trakt API
    /// </summary>
    public struct TraktCredentials : IProviderConfig
    {
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

        [FieldDefinition(5, Label = "OAuth Last Refresh Date", Advanced = true)]
        public DateTime? LastRefreshDate { get; set; }

        private static readonly AbstractValidator<TraktCredentials> Validator = new CredentialsValidation();

        [JsonIgnore()]
        public bool IsSet => !String.IsNullOrEmpty(ClientId);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
    public class CredentialsValidation : AbstractValidator<TraktCredentials>
    {
        public CredentialsValidation()
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
}
