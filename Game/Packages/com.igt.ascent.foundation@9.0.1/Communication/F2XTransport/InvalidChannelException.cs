//-----------------------------------------------------------------------
// <copyright file = "InvalidChannelException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which is thrown when a channel is unknown to the code handling it.
    /// </summary>
    /// <remarks>This exception should only occur when updating the code to support additional channels.</remarks>
    public class InvalidChannelException : Exception
    {
        /// <summary>
        /// The channel which was invalid.
        /// </summary>
        public Channel Channel;

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "Invalid channel: {0}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="channel">The channel which was invalid..</param>
        public InvalidChannelException(Channel channel)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, Enum.GetName(typeof(Channel), channel)))
        {
            Channel = channel;
        }
    }
}
