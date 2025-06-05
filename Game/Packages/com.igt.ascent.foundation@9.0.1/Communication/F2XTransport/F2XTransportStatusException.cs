//-----------------------------------------------------------------------
// <copyright file = "F2XTransportStatusException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when an attempt to register an already registered category is made.
    /// </summary>
    public class F2XTransportStatusException : Exception
    {
        /// <summary>
        /// Dictionary used to lookup descriptions for reply codes.
        /// </summary>
        private static readonly Dictionary<int, string> CodeDescriptions = new Dictionary<int, string>
                                                                               {
                                                                                   {0, "No error occurred."},
                                                                                   {1, "An unspecified error occurred."},
                                                                                   {-1, "The API category was not found."},
                                                                                   {-2, "The bin did not have the token."},
                                                                                   {-3, "No transaction was open."},
                                                                                   {-4, "There was an error decoding the message."},
                                                                                   {-5, "The message decoded successfully but was invalid because of the incorrect message body."},
                                                                                   {-6, "The message decoded successfully but the version was invalid."},
                                                                                   {-7, "The message decoded successfully but the communication channel was not valid."} // This status code applies to a message sent within a transaction.
                                                                               };

        /// <summary>
        /// The status code sent from F2X.
        /// </summary>
        public int StatusCode { private set; get; }

        /// <summary>
        /// A string representation of the status code.
        /// </summary>
        public string StatusDescription { private set; get; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "Received non-zero status code from Foundation. Code: {0}, Descripton: {1}";

        /// <summary>
        /// String indicating that the status code does not have a description.
        /// </summary>
        private const string NoDescription = "The status code does not have a description.";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        public F2XTransportStatusException(int statusCode)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, statusCode, GetStatusDescription(statusCode)))
        {
            
            StatusCode = statusCode;
            StatusDescription = GetStatusDescription(statusCode);
        }

        /// <summary>
        /// Get the description of the status code.
        /// </summary>
        /// <param name="statusCode">Code to get a description for.</param>
        /// <returns>
        /// A description of the code or a string indicating that the status code does not have a description.
        /// </returns>
        private static string GetStatusDescription(int statusCode)
        {
            return CodeDescriptions.ContainsKey(statusCode) ? CodeDescriptions[statusCode] : NoDescription;
        }
    }
}