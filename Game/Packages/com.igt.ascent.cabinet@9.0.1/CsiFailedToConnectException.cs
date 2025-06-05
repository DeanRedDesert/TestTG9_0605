//-----------------------------------------------------------------------
// <copyright file = "CsiFailedToConnectException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when an attempt to connect to the Csi fails.
    /// </summary>
    [Serializable]
    public class CsiFailedToConnectException : Exception
    {
        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "Csi connection attempt failed with error code: {0} Message: {1}.";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="errorCode">The error code returned from the CSI.</param>
        /// <param name="message">The error message returned from the CSI.</param>
        public CsiFailedToConnectException(string errorCode, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, errorCode, message))
        {
        }
    }
}
