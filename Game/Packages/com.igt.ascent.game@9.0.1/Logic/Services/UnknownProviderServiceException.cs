//-----------------------------------------------------------------------
// <copyright file = "UnknownProviderServiceException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Globalization;

    /// <summary>
    ///    Exception to be thrown when a ServiceProvider attribute
    ///    is requested that does not exist.
    /// </summary>
    public class UnknownProviderServiceException : Exception
    {
        private const string MessageFormat = "Service Provider: \"{0}\" Service Name: \"{1}\" Reason: {2}";

        /// <summary>
        ///    Initialize an instance of UnknownProviderMemberException.
        /// </summary>
        public UnknownProviderServiceException() {}

        /// <summary>
        ///    Initialize an instance of UnknownProviderMemberException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that does not have the member.</param>
        /// <param name = "serviceName">Name of the unknown provider member.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public UnknownProviderServiceException(string serviceProviderName, string serviceName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceProviderName, serviceName, message))
        {
            ServiceProviderName = serviceProviderName;
            ServiceName = serviceName;
        }

        /// <summary>
        ///    Initialize an instance of UnknownProviderMemberException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that does not have the member.</param>
        /// <param name = "serviceName">Name of the unknown provider member.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public UnknownProviderServiceException(string serviceProviderName, string serviceName, string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceProviderName, serviceName, message), innerException)
        {
            ServiceProviderName = serviceProviderName;
            ServiceName = serviceName;
        }

        /// <summary>
        ///    Get the name of the service provider.
        /// </summary>
        public string ServiceProviderName
        {
            get;
            private set;
        }

        /// <summary>
        ///    Get the name of the member that is unknown..
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }
    }
}
