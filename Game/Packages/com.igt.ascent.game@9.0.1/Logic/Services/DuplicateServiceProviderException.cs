//-----------------------------------------------------------------------
// <copyright file = "DuplicateServiceProviderException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception is thrown when a new provider is added to the list of providers 
    /// and another provider exists with the same name.
    /// </summary>
    [Serializable]
    public class DuplicateServiceProviderException : Exception
    {
        /// <summary>
        /// Exception message format.
        /// </summary>
        private const string MessageFormat = "Service Name: \"{0}\" Reason: {1}";

        /// <summary>
        ///    Initialize an instance of DuplicateServiceProivderException.
        /// </summary>
        public DuplicateServiceProviderException() {}

        /// <summary>
        ///    Initialize an instance of DuplicateServiceProivderException.
        /// </summary>
        /// <param name = "serviceName">Name of the service provider being added.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public DuplicateServiceProviderException(string serviceName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceName, message))
        {
            ServiceName = serviceName;
        }

        /// <summary>
        ///    Initialize an instance of DuplicateServiceProivderException.
        /// </summary>
        /// <param name = "serviceName">Name of the service provider being added.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public DuplicateServiceProviderException(string serviceName, string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceName, message), innerException)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        ///    The name of the service that is being added.
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }
    }
}
