//-----------------------------------------------------------------------
// <copyright file = "CommunicationErrorEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Event argument for events that are triggered when there is a communication error.
    /// </summary>
    public class CommunicationErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Construct a CommunicationErrorEventArgs.
        /// </summary>
        /// <param name="exception">The exception that occurred during communications.</param>
        /// <param name="rawMessage">The message describing the exception.</param>
        public CommunicationErrorEventArgs(Exception exception, byte[] rawMessage)
            : base()
        {
            CommunicationException = exception;
            RawMessage = rawMessage;
        }

        /// <summary>
        /// The exception that occurred during communications.
        /// </summary>
        public Exception CommunicationException
        {
            get;
            protected set;
        }

        /// <summary>
        /// The raw message that triggered the error condition.
        /// </summary>
        public byte[] RawMessage
        {
            get;
            protected set;
        }
    }
}
