//-----------------------------------------------------------------------
// <copyright file = "TiltCommunicationException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces.TiltControl
{
    using System;
    using System.Globalization;

    ///<summary>
    /// Used to notify that there was an error when posting or clearing tilts with Foundation.
    ///</summary>
    public class TiltCommunicationException : Exception
    {
        /// <summary>
        /// The communication operation during which the tilt was thrown.
        /// </summary>
        public enum Operation
        {
            /// <summary>
            /// Signifies the tilt was thrown during while Posting.
            /// </summary>
            Post,

            /// <summary>
            /// Signifies the tilt was thrown during while Clearing.
            /// </summary>
            Clear
        }

        private const string ExceptionFormat = "Tilt communication error during {0}.  Tilt key: '{1}'";

        /// <summary>
        /// Create an <see cref="TiltCommunicationException"/> with relevant information.
        /// </summary>
        /// <param name="tiltKey">The tilt key that was being operated on.</param>
        /// <param name="operation">The operation during which the tilt was thrown.</param>
        public TiltCommunicationException(string tiltKey, Operation operation)
            : base(string.Format(CultureInfo.InvariantCulture, ExceptionFormat, operation, tiltKey))
        {
        }
    }
}