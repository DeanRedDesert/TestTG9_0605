//-----------------------------------------------------------------------
// <copyright file = "InvalidProgressiveConfigException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;

    /// <summary>
    /// Exception to be thrown when an invalid progressive configuration is encountered.
    /// </summary>
    [Serializable]
    public class InvalidProgressiveConfigException : Exception
    {
        /// <summary>
        /// Construct an InvalidProgressiveConfigException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidProgressiveConfigException(string message)
            : base(message)
        {
        }
    }
}
