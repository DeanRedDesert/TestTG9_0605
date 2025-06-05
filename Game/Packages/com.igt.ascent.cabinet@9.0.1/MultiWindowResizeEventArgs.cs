//-----------------------------------------------------------------------
// <copyright file = "MultiWindowResizeEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating that multiple windows should be resized.
    /// </summary>
    public class MultiWindowResizeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the desired list of window states.
        /// </summary>
        public IList<Window> RequestedWindows { get; }

        /// <summary>
        /// Gets the request ID associated with the event.
        /// </summary>
        public ulong RequestId { get; }

        /// <summary>
        /// Constructs an instance with the given list of window states and request ID.
        /// </summary>
        /// <param name="windows">New information for the windows.</param>
        /// <param name="requestId">The request ID associated with the event.</param>
        public MultiWindowResizeEventArgs(IList<Window> windows, ulong requestId = 0)
        {
            RequestedWindows = windows;
            RequestId = requestId;
        }
    }
}