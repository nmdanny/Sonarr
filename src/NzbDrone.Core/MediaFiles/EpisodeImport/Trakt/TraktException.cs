using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Trakt
{
    public class TraktException : NzbDroneException
    {
        public TraktException(string message) : base(message)
        {
        }

        public TraktException(string message, params object[] args) : base(message, args)
        {
        }

        public TraktException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TraktException(string message, Exception innerException, params object[] args) : base(message, innerException, args)
        {
        }
    }

    public class InvalidCredentialsException : TraktException
    {
        public InvalidCredentialsException(string detail): base($"Failed to connect to Trakt due to invalid credentials: {detail}") { }
    }
    public class TraktCredentialsAlreadyExist : TraktException
    {
        public TraktCredentialsAlreadyExist(): base("Can't have more than one set of Trakt credentials.") { }
    }
    public class MissingTraktCredentials : TraktException
    {
        public MissingTraktCredentials() : base("No Trakt credentials are stored in the database.") { }
    }
}
