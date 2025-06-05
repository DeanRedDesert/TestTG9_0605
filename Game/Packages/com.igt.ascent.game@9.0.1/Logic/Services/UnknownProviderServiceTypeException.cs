//-----------------------------------------------------------------------
// <copyright file = "UnknownProviderServiceTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    ///    Exception to be thrown when a member type is not supported by a function.
    /// </summary>
    public class UnknownProviderServiceTypeException : Exception
    {
        private const string MessageFormat =
            "Service Provider: \"{0}\" Service Name: \"{1}\" Service Type: \"{2}\" Reason: {3}";

        /// <summary>
        ///    Initialize an instance of UnknownProviderMemberTypeException.
        /// </summary>
        public UnknownProviderServiceTypeException()
                {}

        /// <summary>
        ///    Initialize an instance of UnknownProviderMemberTypeException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the member.</param>
        /// <param name = "serviceName">Name of the member that is the unknown type.</param>
        /// <param name = "serviceType">Type of member that is not supported.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public UnknownProviderServiceTypeException(string serviceProviderName, string serviceName,
            MemberTypes serviceType, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceProviderName, serviceName, serviceType, message))
        {
            ServiceProviderName = serviceProviderName;
            ServiceName = serviceName;
            ServiceType = serviceType;
        }

        /// <summary>
        ///    Initialize an instance of UnknownProviderMemberTypeException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the member.</param>
        /// <param name = "serviceName">Name of the member that is the unknown type.</param>
        /// <param name = "serviceType">Type of member that is not supported.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public UnknownProviderServiceTypeException(string serviceProviderName, string serviceName,
            MemberTypes serviceType, string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                    serviceProviderName, serviceName, serviceType, message), innerException)
        {
            ServiceProviderName = serviceProviderName;
            ServiceName = serviceName;
            ServiceType = serviceType;
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
        ///    Get the name of the member that is the unknown type.
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }

        /// <summary>
        ///    Get the member type the is not supported.
        /// </summary>
        public MemberTypes ServiceType
        {
            get;
            private set;
        }
    }
}
