//-----------------------------------------------------------------------
// <copyright file = "InvalidProviderParametersException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;

    /// <summary>
    ///    Exception to be thrown when parameters passed into a ServiceProvider
    ///    are not valid.
    /// </summary>
    public class InvalidProviderParametersException : Exception
    {
        /// <summary>
        ///    Initialize an instance of InvalidProviderParametersException.
        /// </summary>
        public InvalidProviderParametersException()
        { }

        /// <summary>
        ///    Initialize an instance of InvalidProviderParametersException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the member.</param>
        /// <param name = "methodName">Name of the method that has invalid parameters.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        public InvalidProviderParametersException(string serviceProviderName, string methodName, string message)
            : base(message)
        {
            ServiceProviderName = serviceProviderName;
            MethodName = methodName;
        }

        /// <summary>
        ///    Initialize an instance of InvalidProviderParametersException.
        /// </summary>
        /// <param name = "serviceProviderName">Name of the service provider that contains the member.</param>
        /// <param name = "methodName">Name of the method that has invalid parameters.</param>
        /// <param name = "message">Reason for throwing this exception.</param>
        /// <param name = "innerException">Exception that caused this exception.</param>
        public InvalidProviderParametersException(string serviceProviderName, string methodName,
            string message, Exception innerException)
            : base(message, innerException)
        {
            ServiceProviderName = serviceProviderName;
            MethodName = methodName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() +
                " (Service Provider Name: \"" + ServiceProviderName + "\")" +
                " (Method Name: \"" + MethodName + "\")";
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
