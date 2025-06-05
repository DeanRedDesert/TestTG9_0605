//-----------------------------------------------------------------------
// <copyright file = "WindowResizeEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating that the window should be resized.
    /// </summary>
    public class WindowResizeEventArgs : EventArgs
    {
        /// <summary>
        /// The desired window state.
        /// </summary>
        public Window RequestedWindow { get; }

        /// <summary>
        /// Gets the request ID associated with the event.
        /// </summary>
        public ulong RequestId { get; }

        /// <summary>
        /// Construct an instance with the given size.
        /// </summary>
        /// <param name="window">New information for the window.</param>
        public WindowResizeEventArgs(Window window)
        {
            RequestedWindow = window;
        }

        /// <summary>
        /// Construct an instance with a given size and request ID.
        /// </summary>
        /// <param name="window">New information for the window.</param>
        /// <param name="requestId">The request ID associated with the event.</param>
        public WindowResizeEventArgs(Window window, ulong requestId)
            : this(window)
        {
            RequestId = requestId;
        }
    }
}
