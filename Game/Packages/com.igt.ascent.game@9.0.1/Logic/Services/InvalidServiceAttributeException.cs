//-----------------------------------------------------------------------
// <copyright file = "InvalidServiceAttributeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Globalization;

    /// <summary>
    ///    Exception to be thrown when a ServiceProvider member does not
    ///    meet the requirements for the attribute tag associated with it.
    /// </summary>
    public class InvalidServiceAttributeException : Exception
    {
        private const string MessageFormat = "Service Provider: \"{0}\" Service Name: \"{1}\" Reason: {2}";

        /// <summary>
        ///    Initialize an instance of InvalidServiceAttributeException.
        /// </summary>
        public InvalidServiceAttributeException()
        {}

        /// <summary>
        ///    Initialize an instance of InvalidServiceAttributeException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the invalid member.</param>
        /// <param name = "serviceName">The name of the provider member that is invalid.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public InvalidServiceAttributeException(string serviceProviderName, string serviceName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceProviderName, serviceName, message))
        {
            ServiceProviderName = serviceProviderName;
            ServiceName = serviceName;
        }

        /// <summary>
        ///    Initialize an instance of InvalidServiceAttributeException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the invalid member.</param>
        /// <param name = "serviceName">The name of the provider member that is invalid.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public InvalidServiceAttributeException(string serviceProviderName, string serviceName,
                                                string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceProviderName, serviceName, message), innerException)
        {
            ServiceProviderName = serviceProviderName;
            ServiceName = serviceName;
        }

        /// <summary>
        ///    Get the name of the service provider that contains the invalid member.
        /// </summary>
        public string ServiceProviderName
        {
            get;
            private set;
        }

        /// <summary>
        ///    Get the name of the member that is invalid.
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }
    }
}
