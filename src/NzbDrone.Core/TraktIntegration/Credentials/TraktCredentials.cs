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
using System.Net;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.TraktIntegration.Credentials
{
    /// <summary>
    /// Credentials needed for the Trakt API
    /// </summary>
    public class TraktCredentials: ModelBase
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? LastRefreshDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

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

            RuleFor(creds => creds.AccessToken).NotEmpty();
            RuleFor(creds => creds.RefreshToken).NotEmpty();
            RuleFor(creds => creds.LastRefreshDate).NotNull().LessThanOrEqualTo(DateTime.Now);
            RuleFor(creds => creds.ExpirationDate).NotNull().GreaterThanOrEqualTo(DateTime.Now);

        }
    }
}
