//-----------------------------------------------------------------------
// <copyright file = "UnknownServiceProviderException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Globalization;

    /// <summary>
    ///    Exception that should be thrown when a service provider cannot be found.
    /// </summary>
    public class UnknownServiceProviderException : Exception
    {
        private const string MessageFormat = "Service Provider: \"{0}\" Reason: {1}";

        /// <summary>
        ///    Initialize an instance of UnknownServiceProviderException.
        /// </summary>
        public UnknownServiceProviderException()
                {}

        /// <summary>
        ///    Initialize an instance of UnknownServiceProviderException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that does not have the member.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public UnknownServiceProviderException(string serviceProviderName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, serviceProviderName, message))
        {
            ServiceProviderName = serviceProviderName;
        }

        /// <summary>
        ///    Initialize an instance of UnknownServiceProviderException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that does not have the member.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public UnknownServiceProviderException(string serviceProviderName, string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, serviceProviderName, message),
                    innerException)
        {
            ServiceProviderName = serviceProviderName;
        }

        /// <summary>
        ///    Get the name of the service provider.
        /// </summary>
        public string ServiceProviderName
        {
            get;
            private set;
        }
    }
}
