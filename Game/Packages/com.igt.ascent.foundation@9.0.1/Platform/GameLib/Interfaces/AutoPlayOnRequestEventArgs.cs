//-----------------------------------------------------------------------
// <copyright file = "AutoPlayOnRequestEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Event indicating the foundation has requested to turn on the auto play.
    /// </summary>
    [Serializable]
    public class AutoPlayOnRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Get or set the flag indicating if the game accepts the foundation request.
        /// </summary>
        /// <remarks>
        /// This flag should be filled by event handler and checked by event poster. 
        /// event handler must be aware that 
        /// there might be other handlers for this event.
        /// </remarks>
        public bool RequestAccepted { get; set; }
    }
}
