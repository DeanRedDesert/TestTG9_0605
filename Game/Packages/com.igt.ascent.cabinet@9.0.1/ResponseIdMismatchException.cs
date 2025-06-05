//-----------------------------------------------------------------------
// <copyright file = "ResponseIdMismatchException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that a response had an invalid ResponseId.
    /// </summary>
    public class ResponseIdMismatchException : Exception
    {
        /// <summary>
        /// RequestId associated with the exception.
        /// </summary>
        public ulong RequestId { get; }

        /// <summary>
        /// ReponseId associated with the exception.
        /// </summary>
        public ulong ResponseId { get; }

        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "ResponseId ({0}) does not match RequestId ({1}).";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="requestId">The RequestId of the message.</param>
        /// <param name="responseId">The ResponseId of the message.</param>
        public ResponseIdMismatchException(ulong requestId, ulong responseId)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, responseId, requestId))
        {
            RequestId = requestId;
            ResponseId = responseId;
        }

    }
}
