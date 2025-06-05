//-----------------------------------------------------------------------
// <copyright file = "InsufficientDataBufferException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// This exception indicates that a data buffer is not
    /// big enough for a specified operation.
    /// </summary>
    [Serializable]
    public class InsufficientDataBufferException : Exception
    {
        /// <summary>
        /// Initialize an instance of <see cref="InsufficientDataBufferException"/>
        /// with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InsufficientDataBufferException(string message)
            : base(message)
        {
        }
    }
}
