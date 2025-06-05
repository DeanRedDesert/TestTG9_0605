//-----------------------------------------------------------------------
// <copyright file = "MonitorControllerException.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Monitor
{
    using System;

    /// <summary>
    /// Exception thrown by the <see cref="MonitorController"/>
    /// </summary>
    public class MonitorControllerException : Exception
    {
        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">Description of the exception.</param>
        public MonitorControllerException(string message)
            : base(message)
        {
        }
    }
}
