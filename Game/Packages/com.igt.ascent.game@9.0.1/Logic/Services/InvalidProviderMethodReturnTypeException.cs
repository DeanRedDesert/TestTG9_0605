//-----------------------------------------------------------------------
// <copyright file = "InvalidProviderMethodReturnTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Globalization;

    /// <summary>
    ///    Exception to be thrown when the return type of a method is not valid
    ///    for the type of game service attribute it is tagged with.
    /// </summary>
    public class InvalidProviderMethodReturnTypeException : Exception
    {
        private const string MessageFormat = "Service Provider: \"{0}\" Method Name: \"{1}\" Reason: {2}";

        /// <summary>
        ///    Initialize an instance of InvalidProviderMethodReturnTypeException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the member.</param>
        /// <param name = "methodName">Name of the method that has invalid return type.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public InvalidProviderMethodReturnTypeException(string serviceProviderName, string methodName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, serviceProviderName, methodName, message))
        {
            ServiceProviderName = serviceProviderName;
            MethodName = methodName;
        }

        /// <summary>
        ///    Initialize an instance of InvalidProviderMethodReturnTypeException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the member.</param>
        /// <param name = "methodName">Name of the method that has invalid return type.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public InvalidProviderMethodReturnTypeException(string serviceProviderName, string methodName,
            string message, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, serviceProviderName, methodName, message),
                    innerException)
        {
            ServiceProviderName = serviceProviderName;
            MethodName = methodName;
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
        public string MethodName
        {
            get;
            private set;
        }
    }
}
