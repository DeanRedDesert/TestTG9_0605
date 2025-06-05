//-----------------------------------------------------------------------
// <copyright file = "UserDataTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that requested user data could not be accessed as the specified type.
    /// </summary>
    public class UserDataTypeException : Exception
    {
        /// <summary>
        /// The type of user data requested.
        /// </summary>
        public string RequestedType { private set; get; }

        /// <summary>
        /// The name of the requested user data.
        /// </summary>
        public string UserDataName { private set; get; }

        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "The requested user data: {0} could not be read as the requested type: {1}";

        /// <summary>
        /// Create a UserDataTypeException with the specified information.
        /// </summary>
        /// <param name="userDataName">The name of the requested user data.</param>
        /// <param name="requestedType">The type the user data was requested as.</param>
        public UserDataTypeException(string userDataName, string requestedType)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, userDataName, requestedType))
        {
            RequestedType = requestedType;
            UserDataName = userDataName;
        }

        /// <summary>
        /// Create a UserDataTypeException with the specified information.
        /// </summary>
        /// <param name="userDataName">The name of the requested user data.</param>
        /// <param name="requestedType">The type the user data was requested as.</param>
        /// <param name="innerException">The exception which was the originator of this exception.</param>
        public UserDataTypeException(string userDataName, string requestedType, Exception innerException)
            : base(
                string.Format(CultureInfo.InvariantCulture, MessageFormat, userDataName, requestedType), innerException)
        {
            RequestedType = requestedType;
            UserDataName = userDataName;
        }
    }
}
