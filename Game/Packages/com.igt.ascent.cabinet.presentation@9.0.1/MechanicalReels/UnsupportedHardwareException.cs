//-----------------------------------------------------------------------
// <copyright file = "UnsupportedHardwareException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;

    /// <summary>
    /// UnsupportedHardwareException
    /// </summary>
    [Serializable]
    public class UnsupportedHardwareException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public UnsupportedHardwareException()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Optional message to set in the exception.</param>
        public UnsupportedHardwareException(string message)
            : base(message)
        { }
    }
}
