//-----------------------------------------------------------------------
// <copyright file = "CommunicationInterfaceUninitializedException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;

    /// <summary>
    /// The exception thrown when a communication interface is used
    /// before being initialized.
    /// </summary>
    [Serializable]
    internal sealed class CommunicationInterfaceUninitializedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CommunicationInterfaceUninitializedException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CommunicationInterfaceUninitializedException(string message)
            : base(message)
        {
        }
    }
}
