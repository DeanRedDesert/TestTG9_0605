//-----------------------------------------------------------------------
// <copyright file = "FoundationReplyException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that a reply from the foundation contained a non-zero reply code.
    /// </summary>
    public class FoundationReplyException : Exception
    {
        /// <summary>
        /// Get the reply code from the foundation reply.
        /// </summary>
        public int ReplyCode { private set; get; }

        /// <summary>
        /// Get the error description from the reply.
        /// </summary>
        public string ErrorDescription { private set; get; }

        /// <summary>
        /// Message format used to form the exception message.
        /// </summary>
        private const string MessageFormat = "Foundation call failure: Reply Code: {0} Description: {1}";

        /// <summary>
        /// Construct an instance of the exception with the given parameters.
        /// </summary>
        /// <param name="replyCode">Reply code which was in the reply.</param>
        /// <param name="errorDescription">Error description from the reply.</param>
        public FoundationReplyException(int replyCode, string errorDescription) : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, replyCode, errorDescription))
        {
            ReplyCode = replyCode;
            ErrorDescription = errorDescription;
        }
    }
}
