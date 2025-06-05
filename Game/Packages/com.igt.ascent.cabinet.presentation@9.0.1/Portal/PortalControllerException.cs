//-----------------------------------------------------------------------
// <copyright file = "PortalControllerException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Portal
{
    using System;

    /// <summary>
    /// Exception thrown by the <see cref="PortalController"/>.
    /// </summary>
    public class PortalControllerException : Exception
    {
        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">Description of the exception.</param>
        public PortalControllerException(string message) : base(message)
        {
        }
    }
}